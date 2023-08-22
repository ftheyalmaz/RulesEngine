using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RuleEngine
{
    internal class Program
    {

        static void Main(string[] args)
        {
            //Setting up the database interaction object and connecting to the DB
            string connectionString = "Server=dev1.layeronesoftware.com;Database=FTBTrade;Integrated Security=SSPI;";
            DatabaseInteraction dbInteraction = new DatabaseInteraction(connectionString);
            dbInteraction.OpenConnection();


            //Records on which to run our compliance rules and expressions
            List<TargetContext> tempPQLResults = dbInteraction.GetAllFromTable("dbo.TempPQLResult");
            //dbInteraction.DisplayTradeRecords();


            //Setting up the context/scope of the expression in the form
            //of parameters we pass upon execution, for now the only provided context
            //is the record on which we are applying our rules
            List<HFLib.ContextParameter> parameters = new List<HFLib.ContextParameter>();
            parameters.Add(new HFLib.ContextParameter { Name = "TargetContext", ParameterType = typeof(TargetContext) });


            //Here we declare our string expression and test ParseExpressionStrToTree()
            const string exp = "TargetContext.StartPriceN > 50 && TargetContext.Manager == \"Courtney Carson\"";
            Expression parseTree = HFLib.ParseExpressionStrToTree(exp, parameters);// [1] WEB API endpoint
                                                                                   //Console.WriteLine("exp: " + parseTree.ToString());
                                                                                   //HFLib.PrintPreOrder(parseTree);

            /*
             * on startup cache rules , execute as they come along, [2] custom optimized parse tree
             */

            //Here we upload the tree to the database
            dbInteraction.PostExpressionToDatabase(parseTree); //[1.5]keep autoincrement and do the post/get
            ////, [3] custom execute, dynamic context loading

            //Here we test ExecuteOn() on parseTree with each record in TempPQLResult,
            //and output results  
            foreach (TargetContext record in tempPQLResults)
            {
                bool passed = HFLib.ExecuteOn(parseTree, record);
                string toPrint = passed ? "COMPLIANCE PASSED: " : "COMPLIANCE FAILED: ";
                toPrint += "{Strategy: " + record.Strategy;
                toPrint += ", Security Type: " + record.SecurityType;
                toPrint += ", Clearing Broker: " + record.ClearingBroker;
                toPrint += ", Sid: " + record.Sid;
                toPrint += ", Start Price [N]: " + record.StartPriceN;
                toPrint += ", Manager: " + record.Manager + "}";
                Console.WriteLine(toPrint);
            }

            Console.ReadLine();

            dbInteraction.CloseConnection();
        }


    }
    
}






