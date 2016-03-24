using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleParserTest
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> testRules = new List<string>();
            //testRules.Add("x == '1'");
            //testRules.Add("x == 'test ");
            //testRules.Add("x == ");
            //testRules.Add("x");
            //testRules.Add("x == ''");
            //testRules.Add("(x == '1')");
            //testRules.Add("x == '1' AND y == '2'");
            //testRules.Add("(x == '1') AND y == '2'");
            //testRules.Add("( x == '1' ) AND ( y == '2' )");
            //testRules.Add("x == '1' AND y == '2' AND z == '3'");
            //testRules.Add("x == '1' AND y == '2' OR z == '3'");
            //testRules.Add("x == '1' AND (y == '2' OR z == '3')");
            //testRules.Add("(x == '1' AND y == '2') OR z == '3'");
            testRules.Add("(x == '1' AND y == '2') OR (z == '3' OR w == '55' OR foo == 'bar')");
            testRules.Add("(x == '1' AND y == '2' OR (z == '3' OR w == '55' OR foo == 'bar')");
            testRules.Add("(x == '1' AND y == '2') OR ((z == '3' OR w == '55' OR foo == 'bar')");
            testRules.Add("(x == '1' AND y == '2') OR (z == '3' OR w == '55' OR foo == 'bar'");
            testRules.Add("(x == '1' AND y == '2') OR z == '3' OR w == '55' OR foo == 'bar')");
            testRules.Add("a == 'bob' OR (x == '1') OR h == 'hat' OR (z == '3' OR w == '55' OR foo == 'bar' OR (mycar == 'cool' AND love == 'surf'))");
            testRules.Add(@"     a == 'bob' OR
     (
      x == '1' AND y == '2'
     )
     OR h == 'hat' OR
     (
       z == '3' OR w == '55' OR foo == 'bar' OR
       (
        mycar == 'cool' AND love == 'surf'
       )   )
");

            foreach (string ruleStr in testRules)
            {
                Console.WriteLine("Original: " + ruleStr);
                Console.WriteLine("------");
                try
                {
                    RuleList list = RuleList.ParseRules(ruleStr);
                    Console.WriteLine(list.ToString(5));
                }
                catch (Exception x)
                {
                    Console.WriteLine("ERROR: " + x.Message);
                }
                Console.WriteLine("----------------------------------");
            }


            Console.WriteLine("DONE!");
            Console.ReadKey();
        }
    }

    interface RuleNode
    {
        //nothing
    }

    class Rule:RuleNode
    {
        public string FieldName = "";
        public string Operator = "";
        public string FieldValue = "";
        public override string ToString()
        {
            return FieldName + " " + Operator + " '" + FieldValue + "'";
        }
    }
    class RuleList:RuleNode
    {
        public enum ConjunctionTypes {NONE, AND, OR };
        public List<RuleNode> Rules = new List<RuleNode>();
        public ConjunctionTypes ListType = ConjunctionTypes.NONE;
        private enum ParseState { FIELD, OPERATOR, VALUE, CONJUNCT };
        public override string ToString()
        {
            return ToString(0);
        }
        public string ToString(int indentLevel)
        {
            string padding = new string(' ',indentLevel);
            StringBuilder result = new StringBuilder();
            result.Append(padding);
            for (int c = 0; c < Rules.Count; c++)
            {
                var cur = Rules[c];
                if (cur is Rule)
                {
                    string curStr = ((Rule)cur).ToString();
                    result.Append(curStr);
                    if (c < (Rules.Count - 1))
                    {
                        result.Append(" " + this.ListType.ToString() + " ");
                    }
                }
                else if (cur is RuleList)
                {
                    if (c > 0) result.Append("\r\n"+padding);
                    result.AppendLine("(");
                    result.AppendLine(((RuleList)cur).ToString(++indentLevel));
                    result.AppendLine(padding + ")");
                    if (c < (Rules.Count - 1))
                    {
                        result.Append(padding + this.ListType.ToString() + " ");
                    }
                }
            }
            return result.ToString();
        }

        /***************** IDEA:  USE PARENTHESIS AND AN INTEGER LEVEL TO CREATE HIERARCHY OF AND/OR RULES ********************/

        public static RuleList ParseRules(string ruleString)
        {
            if (string.IsNullOrEmpty(ruleString)) return null;
            ParseState curState = ParseState.FIELD;
            int charPtr = 0;
            ruleString = ruleString.Trim();
            string curPiece = "";
            //keep track of the level we are currently working on
            Stack<RuleList> ListTree = new Stack<RuleList>();
            Rule curRule = null;
            RuleList curList = new RuleList();
            int quoteCount = 0;
            while (charPtr < ruleString.Length)
            {
                char curChar = ruleString[charPtr];
                {
                    switch (curState)
                    {
                        case ParseState.FIELD:
                            //check for opening parenthesis - new list
                            if (curChar == '(')
                            {
                                ListTree.Push(curList);
                                curList = new RuleList();
                            }
                            else
                            {
                                if (curChar != ' ')
                                {
                                    curPiece += curChar;
                                    curPiece = curPiece.Trim();
                                }
                                else if (curPiece.Length > 0)
                                {
                                    if (curRule == null) curRule = new Rule();
                                    curRule.FieldName = curPiece;
                                    curPiece = "";
                                    curState = ParseState.OPERATOR;
                                }
                            }
                            break;
                        case ParseState.OPERATOR:
                            if (curChar != ' ')
                            {
                                curPiece += curChar;
                                curPiece = curPiece.Trim();
                            }
                            else if (curPiece.Length > 0)
                            {
                                curRule.Operator = curPiece;
                                curPiece = "";
                                curState = ParseState.VALUE;
                            }
                            break;
                        case ParseState.VALUE:
                            if (curChar == '\'')
                            {
                                quoteCount++;
                            }
                            else
                            {
                                //it's not a quote, so append the value
                                curPiece += curChar;
                            }
                            if (quoteCount > 1)
                            {
                                //it was the second quote, so we must be at the end
                                quoteCount = 0;
                                curRule.FieldValue = curPiece;
                                curList.Rules.Add(curRule);
                                curRule = null;
                                curPiece = "";
                                curState = ParseState.CONJUNCT;
                            }
                            //else it was a quote but we had no value yet, so just continue
                            break;
                        case ParseState.CONJUNCT:
                            //check for closing parenthesis first - end of this RuleList
                            if (curChar == ')')
                            {
                                if (ListTree.Count > 0)
                                {
                                    RuleList parentList = ListTree.Pop();
                                    parentList.Rules.Add(curList);
                                    curList = parentList;
                                }
                                else
                                {
                                    throw new Exception("Mismatched paren near character " + charPtr.ToString());
                                }
                            }
                            else
                            {
                                if (curChar != ' ')
                                {
                                    //it's not a space, so it's part of the conjuction
                                    curPiece += curChar;
                                    curPiece = curPiece.Trim();
                                }
                                else if (curPiece.Length > 0)
                                {
                                    //it is a space and we already have the conjuction
                                    CheckOrSetRuleListConjuction(curPiece, curList);
                                    curPiece = "";
                                    curState = ParseState.FIELD;
                                }
                            }
                            break;
                    }
                }
                charPtr++;
            }
            if (ListTree.Count > 0)
            {
                throw new Exception("Mismatched paren in expression");
            }
            if (curState != ParseState.CONJUNCT)
            {
                throw new Exception("Could not completely parse rules expression, last parsed " + curState.ToString());
            }
            if (!String.IsNullOrEmpty(curPiece))
            {
                throw new Exception("Could not completely parse rules expression, have left over string: " + curPiece);
            }
            return curList;
        }

        private static void CheckOrSetRuleListConjuction(string conjString, RuleList curList)
        {
            ConjunctionTypes curConj = ConjunctionTypes.NONE;
            conjString = conjString.ToUpper().Trim();
            if (conjString == "AND")
            {
                curConj = ConjunctionTypes.AND;
            }
            else if (conjString == "OR")
            {
                curConj = ConjunctionTypes.OR;
            }
            else
            {
                throw new Exception("Invalid RuleList Conjunction Type: " + conjString);
            }
            if (curList.ListType == ConjunctionTypes.NONE)
            {
                curList.ListType = curConj;
            }
            else if (curList.ListType != curConj)
            {
                throw new Exception("Mismatched RuleList Conjunction Type: " + curList.ListType.ToString() + " vs " + curConj.ToString());
            }
        }
    }
}
