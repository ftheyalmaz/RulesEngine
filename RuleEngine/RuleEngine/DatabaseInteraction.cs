using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RuleEngine
{
    internal class DatabaseInteraction
    {
        private string connectionString;

        private SqlConnection dbConnection;


        public DatabaseInteraction(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void OpenConnection()
        {
            dbConnection = new SqlConnection(connectionString);
            dbConnection.Open();  
        }

        public void CloseConnection()
        {
            dbConnection.Close();
        }

        public static RuleDatabaseObjects.ExpressionType CreateExpressionType(string desc)
        {
            return new RuleDatabaseObjects.ExpressionType
            {
                Description = desc,
                Created = DateTime.Now,
                Version = 1,
                LastUpdate = DateTime.Now,
                ApplicationSource = "AppSourcePlaceholder",
                SystemSource = "SystemSourcePlaceholder",
                GlobalVersion = 1
            };
        }

        public static RuleDatabaseObjects.ExpressionValueType CreateExpressionValueType(string desc)
        {
            return new RuleDatabaseObjects.ExpressionValueType
            {
                Description = desc,
                Created = DateTime.Now,
                Version = 1,
                LastUpdate = DateTime.Now,
                ApplicationSource = "AppSourcePlaceholder",
                SystemSource = "SystemSourcePlaceholder",
                GlobalVersion = 1
            };
        }

        public static RuleDatabaseObjects.Expression CreateExpression(string expVal, int pId,
            int tId, int vId, int seq)
        {
            if (pId == 0)
            {
                return new RuleDatabaseObjects.Expression
                {
                    // FIX RuleId, 
                    //and ExpressionCallRuleId
                    RuleId = 7,
                    ExpressionSequence = seq,
                    ExpressionTypeId = tId,
                    ExpressionValue = expVal,
                    ExpressionValueType = vId,
                    Created = DateTime.Now,
                    Version = 1,
                    LastUpdate = DateTime.Now,
                    ApplicationSource = "AppSourcePlaceholder",
                    SystemSource = "SystemSourcePlaceholder",
                    GlobalVersion = 1
                };
            }
            return new RuleDatabaseObjects.Expression
            {
                // FIX RuleId,
                //and ExpressionCallRuleId
                RuleId = 7, 
                ParentExpressionId = pId,
                ExpressionSequence = seq,
                ExpressionTypeId = tId,
                ExpressionValue = expVal,
                ExpressionValueType = vId,
                Created = DateTime.Now,
                Version = 1,
                LastUpdate = DateTime.Now,
                ApplicationSource = "AppSourcePlaceholder",
                SystemSource = "SystemSourcePlaceholder",
                GlobalVersion = 1
            };
        }

        internal void PostExpressionToDatabase(System.Linq.Expressions.Expression e, int pId = 0, int seq = 0)
        {
            if (e == null) return;
            //Upload the expType,valType and expr on DB and get back the id of the created expr
            int p = PostExprNode(e, pId, seq);
            // Get child expressions and recursively add them
            int s = 0;
            foreach (var child in HFLib.GetChildExpressions(e))
            {
                PostExpressionToDatabase(child, p, s++); 
            }
        }

        internal int PostExprNode(System.Linq.Expressions.Expression e, int pId = 0, int seq = 0)
        {
            int tId = PostTNode(e.NodeType.ToString());
            int vId = PostVNode(e.Type.ToString());
            RuleDatabaseObjects.Expression ePut = CreateExpression(e.ToString(), pId, tId, vId, seq);
            int rValue = 0;

            /*
                    Removed duplicate checks, as when inserting x+1, followed by y/1, the expr for 1 in DB
                    will have one parentExpressionId, which is obviously incorrect. However, now if we have
                    +1 in a thousand places we store a thousand 1 exprs. Not sure if this is ok.
            */
            /* Check if e.ToString() = ExpressionValue of any existing entry in Rules.Expression
            //string checkExistenceSql = "SELECT expressionId FROM [Rules].Expression WHERE ExpressionValue = @ExpressionValue";
            //using (SqlCommand cmd = new SqlCommand(checkExistenceSql, dbConnection))
            //{
            //    cmd.Parameters.AddWithValue("@ExpressionValue", e.ToString());
            //    object result = cmd.ExecuteScalar();
            //    if (result != null && result != DBNull.Value)
            //    {
            //        rValue = (int)result;
            //        return rValue;
            //    }
            //}*/

            //INSERT INTO with ePut, then immediately query the expressionId, place that in rValue
            string insertExpression = @"INSERT INTO [Rules].Expression(RuleId, ParentExpressionId, ExpressionSequence, ExpressionTypeId, ExpressionValue, ExpressionValueType, Created, Version, LastUpdate, Application_source, System_source, _GlobalVersion) 
                               VALUES(@RuleId, @ParentExpressionId, @ExpressionSequence, @ExpressionTypeId, @ExpressionValue, @ExpressionValueType, @Created, @Version, @LastUpdate, @ApplicationSource, @SystemSource, @GlobalVersion);
                               SELECT SCOPE_IDENTITY()"; // SELECT SCOPE_IDENTITY() will return the ID of the inserted record
            using (SqlCommand cmd = new SqlCommand(insertExpression, dbConnection))
            {
                cmd.Parameters.AddWithValue("@RuleId", ePut.RuleId);
                cmd.Parameters.AddWithValue("@ParentExpressionId", ePut.ParentExpressionId.HasValue ? (object)ePut.ParentExpressionId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@ExpressionSequence", ePut.ExpressionSequence);
                cmd.Parameters.AddWithValue("@ExpressionTypeId", ePut.ExpressionTypeId);
                cmd.Parameters.AddWithValue("@ExpressionValue", ePut.ExpressionValue);
                cmd.Parameters.AddWithValue("@ExpressionValueType", ePut.ExpressionValueType.HasValue ? (object)ePut.ExpressionValueType.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Created", ePut.Created);
                cmd.Parameters.AddWithValue("@Version", ePut.Version);
                cmd.Parameters.AddWithValue("@LastUpdate", ePut.LastUpdate);
                cmd.Parameters.AddWithValue("@ApplicationSource", ePut.ApplicationSource);
                cmd.Parameters.AddWithValue("@SystemSource", ePut.SystemSource);
                cmd.Parameters.AddWithValue("@GlobalVersion", ePut.GlobalVersion);

                rValue = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return rValue; // Return ePut or existing expressionId from DB
        }


        internal int PostTNode(string desc)
        {
            RuleDatabaseObjects.ExpressionType eType = CreateExpressionType(desc);
            int rValue = 0;

            // Check if desc = Description of any existing entry in Rules.ExpressionType
            string checkExistenceSql = "SELECT expressionTypeId FROM Rules.ExpressionType WHERE Description = @Description";
            using (SqlCommand cmd = new SqlCommand(checkExistenceSql, dbConnection))
            {
                cmd.Parameters.AddWithValue("@Description", desc);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    rValue = (int)result;
                    return rValue;
                }
            }

            // If no entry exists, INSERT INTO with eType, then immediately query the ExpressionTypeId, place that in rValue
            string insertExpressionType = @"INSERT INTO Rules.ExpressionType(Description, Created, Version, LastUpdate, Application_source, System_source, _GlobalVersion) 
                                   VALUES(@Description, @Created, @Version, @LastUpdate, @ApplicationSource, @SystemSource, @GlobalVersion);
                                   SELECT SCOPE_IDENTITY()";
            using (SqlCommand cmd = new SqlCommand(insertExpressionType, dbConnection))
            {
                cmd.Parameters.AddWithValue("@Description", eType.Description);
                cmd.Parameters.AddWithValue("@Created", eType.Created);
                cmd.Parameters.AddWithValue("@Version", eType.Version);
                cmd.Parameters.AddWithValue("@LastUpdate", eType.LastUpdate);
                cmd.Parameters.AddWithValue("@ApplicationSource", eType.ApplicationSource);
                cmd.Parameters.AddWithValue("@SystemSource", eType.SystemSource);
                cmd.Parameters.AddWithValue("@GlobalVersion", eType.GlobalVersion);

                rValue = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return rValue;
        }


        internal int PostVNode(string desc)
        {
            RuleDatabaseObjects.ExpressionValueType eValueType = CreateExpressionValueType(desc);
            int rValue = 0;

            // Check if desc = Description of any existing entry in Rules.ExpressionValueType
            string checkExistenceSql = "SELECT expressionValueTypeId FROM Rules.ExpressionValueType WHERE Description = @Description";
            using (SqlCommand cmd = new SqlCommand(checkExistenceSql, dbConnection))
            {
                cmd.Parameters.AddWithValue("@Description", desc);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    rValue = (int)result;
                    return rValue;
                }
            }

            // If no entry exists, INSERT INTO with eValueType, then immediately query the ExpressionValueTypeId, place that in rValue
            string insertExpressionValueType = @"INSERT INTO Rules.ExpressionValueType(Description, Created, Version, LastUpdate, Application_source, System_source, _GlobalVersion) 
                                        VALUES(@Description, @Created, @Version, @LastUpdate, @ApplicationSource, @SystemSource, @GlobalVersion);
                                        SELECT SCOPE_IDENTITY()";
            using (SqlCommand cmd = new SqlCommand(insertExpressionValueType, dbConnection))
            {
                cmd.Parameters.AddWithValue("@Description", eValueType.Description);
                cmd.Parameters.AddWithValue("@Created", eValueType.Created);
                cmd.Parameters.AddWithValue("@Version", eValueType.Version);
                cmd.Parameters.AddWithValue("@LastUpdate", eValueType.LastUpdate);
                cmd.Parameters.AddWithValue("@ApplicationSource", eValueType.ApplicationSource);
                cmd.Parameters.AddWithValue("@SystemSource", eValueType.SystemSource);
                cmd.Parameters.AddWithValue("@GlobalVersion", eValueType.GlobalVersion);

                rValue = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return rValue;
        }


        public List<TargetContext> GetAllFromTable(string table)
        {
            List<TargetContext> result = new List<TargetContext>();
            string query = $"SELECT * FROM {table}";

            using (SqlCommand command = new SqlCommand(query, dbConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TargetContext trade = new TargetContext
                        {
                            Strategy = reader["Strategy"].ToString(),
                            SecurityType = reader["Security Type"].ToString(),
                            ClearingBroker = reader["Clearing Broker"].ToString(),
                            Sid = (int)reader["Sid"],
                            Ticker = reader["Ticker"].ToString(),
                            SecurityDescription = reader["Security Description"].ToString(),
                            StartPriceN = Convert.ToDouble(reader["Start Price N"]),
                            EndPriceN = Convert.ToDouble(reader["End Price N"]),
                            EndQuantity = reader["End Quantity"].ToString(),
                            QuantityChange = Convert.ToDouble(reader["Quantity Change"]),
                            StartOTE = reader["Start OTE"].ToString(),
                            EndOTE = reader["End OTE"].ToString(),
                            RealizedPL = Convert.ToDouble(reader["Realized P&L"]),
                            TotalPL = Convert.ToDouble(reader["Total P&L"]),
                            EndMarketValue = reader["End Market Value"].ToString(),
                            Manager = reader["Manager"].ToString(),
                            LegalEntity = reader["Legal Entity"].ToString()
                        };
                        result.Add(trade);
                        //Console.WriteLine("{" + $"Strategy: {trade.Strategy}, Security Type: {trade.SecurityType}, Clearing Broker: {trade.ClearingBroker}, Sid: {trade.Sid}, Ticker: {trade.Ticker}, Security Description: {trade.SecurityDescription}, Start Price N: {trade.StartPriceN}, End Price N: {trade.EndPriceN}, End Quantity: {trade.EndQuantity}, Quantity Change: {trade.QuantityChange}, Start OTE: {trade.StartOTE}, End OTE: {trade.EndOTE}, Realized P&L: {trade.RealizedPL}, Total P&L: {trade.TotalPL}, End Market Value: {trade.EndMarketValue}, Manager: {trade.Manager}, Legal Entity: {trade.LegalEntity}" + "}");
                    }
                }
            }
            return result;
        }

        public void DisplayTradeRecords()
        {
            string query = "SELECT * FROM dbo.TempPQLResult";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TargetContext trade = new TargetContext
                            {
                                Strategy = reader["Strategy"].ToString(),
                                SecurityType = reader["Security Type"].ToString(),
                                ClearingBroker = reader["Clearing Broker"].ToString(),
                                Sid = (int)reader["Sid"],
                                Ticker = reader["Ticker"].ToString(),
                                SecurityDescription = reader["Security Description"].ToString(),
                                StartPriceN = Convert.ToDouble(reader["Start Price N"]),
                                EndPriceN = Convert.ToDouble(reader["End Price N"]),
                                EndQuantity = reader["End Quantity"].ToString(),
                                QuantityChange = Convert.ToDouble(reader["Quantity Change"]),
                                StartOTE = reader["Start OTE"].ToString(),
                                EndOTE = reader["End OTE"].ToString(),
                                RealizedPL = Convert.ToDouble(reader["Realized P&L"]),
                                TotalPL = Convert.ToDouble(reader["Total P&L"]),
                                EndMarketValue = reader["End Market Value"].ToString(),
                                Manager = reader["Manager"].ToString(),
                                LegalEntity = reader["Legal Entity"].ToString()
                            };

                            Console.WriteLine("{"+$"Strategy: {trade.Strategy}, Security Type: {trade.SecurityType}, Clearing Broker: {trade.ClearingBroker}, Sid: {trade.Sid}, Ticker: {trade.Ticker}, Security Description: {trade.SecurityDescription}, Start Price N: {trade.StartPriceN}, End Price N: {trade.EndPriceN}, End Quantity: {trade.EndQuantity}, Quantity Change: {trade.QuantityChange}, Start OTE: {trade.StartOTE}, End OTE: {trade.EndOTE}, Realized P&L: {trade.RealizedPL}, Total P&L: {trade.TotalPL}, End Market Value: {trade.EndMarketValue}, Manager: {trade.Manager}, Legal Entity: {trade.LegalEntity}"+"}");
                        }
                    }
                }
            }
        }

        public void DisplayRuleTypeRecords()
        {
            string query = "SELECT * FROM Rules.RuleType";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RuleDatabaseObjects.RuleType ruleType = new RuleDatabaseObjects.RuleType
                            {
                                RuleTypeId = (int)reader["RuleTypeId"],
                                Description = reader["Description"].ToString(),
                                Created = reader["Created"] as DateTime?,
                                Version = reader["Version"] as int?,
                                LastUpdate = reader["LastUpdate"] as DateTime?,
                                ApplicationSource = reader["Application_Source"].ToString(),
                                SystemSource = reader["System_Source"].ToString(),
                                GlobalVersion = reader["_GlobalVersion"] as int?
                            };

                            Console.WriteLine("{"+$"RuleTypeId: {ruleType.RuleTypeId}, Description: {ruleType.Description}, Created: {ruleType.Created}, Version: {ruleType.Version}, LastUpdate: {ruleType.LastUpdate}, ApplicationSource: {ruleType.ApplicationSource}, SystemSource: {ruleType.SystemSource}, GlobalVersion: {ruleType.GlobalVersion}"+"}");
                        }
                    }
                }
            }
        }

        public void DisplayRuleRecords()
        {
            string query = "SELECT * FROM Rules.[Rule]";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RuleDatabaseObjects.Rule rule = new RuleDatabaseObjects.Rule
                            {
                                RuleId = (int)reader["RuleId"],
                                RuleName = reader["RuleName"].ToString(),
                                RuleTypeId = (int)reader["RuleTypeId"],
                                RuleCode = reader["RuleCode"].ToString(),
                                Created = reader["Created"] as DateTime?,
                                Version = reader["Version"] as int?,
                                LastUpdate = reader["LastUpdate"] as DateTime?,
                                ApplicationSource = reader["Application_Source"].ToString(),
                                SystemSource = reader["System_Source"].ToString(),
                                GlobalVersion = reader["_GlobalVersion"] as int?
                            };

                            Console.WriteLine("{"+$"RuleId: {rule.RuleId}, RuleName: {rule.RuleName}, RuleTypeId: {rule.RuleTypeId}, RuleCode: {rule.RuleCode}, Created: {rule.Created}, Version: {rule.Version}, LastUpdate: {rule.LastUpdate}, ApplicationSource: {rule.ApplicationSource}, SystemSource: {rule.SystemSource}, GlobalVersion: {rule.GlobalVersion}"+"}");
                        }
                    }
                }
            }
        }

        public void DisplayExpressionRecords()
        {
            string query = "SELECT * FROM Rules.Expression";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RuleDatabaseObjects.Expression expression = new RuleDatabaseObjects.Expression
                            {
                                ExpressionId = (int)reader["ExpressionId"],
                                RuleId = (int)reader["RuleId"],
                                ParentExpressionId = reader["ParentExpressionId"] as int?,
                                ExpressionSequence = reader["ExpressionSequence"] as int?,
                                ExpressionTypeId = (int)reader["ExpressionTypeId"],
                                ExpressionCallRuleId = reader["ExpressionCallRuleId"] as int?,
                                ExpressionValue = reader["ExpressionValue"].ToString(),
                                ExpressionValueType = reader["ExpressionValueType"] as int?,
                                Created = reader["Created"] as DateTime?,
                                Version = reader["Version"] as int?,
                                LastUpdate = reader["LastUpdate"] as DateTime?,
                                ApplicationSource = reader["Application_Source"].ToString(),
                                SystemSource = reader["System_Source"].ToString(),
                                GlobalVersion = reader["_GlobalVersion"] as int?
                            };

                            Console.WriteLine("{"+$"ExpressionId: {expression.ExpressionId}, RuleId: {expression.RuleId}, ParentExpressionId: {expression.ParentExpressionId}, ExpressionSequence: {expression.ExpressionSequence}, ExpressionTypeId: {expression.ExpressionTypeId}, ExpressionCallRuleId: {expression.ExpressionCallRuleId}, ExpressionValue: {expression.ExpressionValue}, ExpressionValueType: {expression.ExpressionValueType}, Created: {expression.Created}, Version: {expression.Version}, LastUpdate: {expression.LastUpdate}, ApplicationSource: {expression.ApplicationSource}, SystemSource: {expression.SystemSource}, GlobalVersion: {expression.GlobalVersion}"+"}");
                        }
                    }
                }
            }
        }

        public void DisplayExpressionTypeRecords()
        {
            string query = "SELECT * FROM Rules.ExpressionType";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RuleDatabaseObjects.ExpressionType expressionType = new RuleDatabaseObjects.ExpressionType
                            {
                                ExpressionTypeId = (int)reader["ExpressionTypeId"],
                                Description = reader["Description"].ToString(),
                                Created = reader["Created"] as DateTime?,
                                Version = reader["Version"] as int?,
                                LastUpdate = reader["LastUpdate"] as DateTime?,
                                ApplicationSource = reader["Application_Source"].ToString(),
                                SystemSource = reader["System_Source"].ToString(),
                                GlobalVersion = reader["_GlobalVersion"] as int?
                            };

                            Console.WriteLine("{"+$"ExpressionTypeId: {expressionType.ExpressionTypeId}, Description: {expressionType.Description}, Created: {expressionType.Created}, Version: {expressionType.Version}, LastUpdate: {expressionType.LastUpdate}, ApplicationSource: {expressionType.ApplicationSource}, SystemSource: {expressionType.SystemSource}, GlobalVersion: {expressionType.GlobalVersion}"+"}");
                        }
                    }
                }
            }
        }

        public void DisplayExpressionValueTypeRecords()
        {
            string query = "SELECT * FROM Rules.ExpressionValueType";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RuleDatabaseObjects.ExpressionValueType expressionValueType = new RuleDatabaseObjects.ExpressionValueType
                            {
                                ExpressionValueTypeId = (int)reader["ExpressionValueTypeId"],
                                Description = reader["Description"].ToString(),
                                Created = reader["Created"] as DateTime?,
                                Version = reader["Version"] as int?,
                                LastUpdate = reader["LastUpdate"] as DateTime?,
                                ApplicationSource = reader["Application_Source"].ToString(),
                                SystemSource = reader["System_Source"].ToString(),
                                GlobalVersion = reader["_GlobalVersion"] as int?
                            };

                            Console.WriteLine("{"+$"ExpressionValueTypeId: {expressionValueType.ExpressionValueTypeId}, Description: {expressionValueType.Description}, Created: {expressionValueType.Created}, Version: {expressionValueType.Version}, LastUpdate: {expressionValueType.LastUpdate}, ApplicationSource: {expressionValueType.ApplicationSource}, SystemSource: {expressionValueType.SystemSource}, GlobalVersion: {expressionValueType.GlobalVersion}"+"}");
                        }
                    }
                }
            }
        }

        
    }
}
