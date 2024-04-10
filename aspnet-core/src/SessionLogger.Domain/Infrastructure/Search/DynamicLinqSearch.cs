using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Infrastructure.Search
{
    public static class DynamicLinqSearch {

        public static string GetMultiFieldWHERE(List<string> searchFieldNames, string keywords, List<string> noise = null)
        {
            if (noise == null)
            {
                noise = new List<string>();
            }

            var terms = GetTerms(keywords, noise);

            int termPointer = 0;

            return GetMultiFieldWHEREInner(terms, searchFieldNames, ref termPointer);
        }

        public static List<string> GetTerms(string keywords, List<string> noise)
        {
            int i = 0;
            bool finishedTerm = false;
            bool inQuote = false;
            string query = keywords;
            string term = "";
            bool termHadQuotes = false;
            var aterms = new List<string>();
            while (i < query.Length)
            {
                finishedTerm = false;
                i++;
                var c = query.Substring(i - 1, 1);

                if (c == "\"")
                {
                    //it's a quote

                    // is it a pair of quotes?
                    bool blnPair = false;
                    if (i < query.Length)
                    {
                        if (query.Substring(i, 1) == "\"")
                        {
                            // there's a quote after this
                            blnPair = true;
                        }
                    }
                    if (blnPair)
                    {
                        c = "\"";
                        i++;
                    }
                    else
                    {
                        // single quote
                        inQuote = !inQuote;
                        if (inQuote)
                        {
                            // now coming out of a quoted phrase
                            finishedTerm = true;
                        }
                        else
                        {
                            termHadQuotes = true;
                        }
                    }

                    // don't let quote through
                    c = "";
                }

                var ccopy = c;
                if (c == " " || c == "," || c == "(" || c == ")" && !inQuote)
                {
                    // these characters finish a term unless within a quote
                    c = "";
                    finishedTerm = true;
                }
                if (c == "-" && term.Trim() == "" && !inQuote)
                {
                    // - is the same as AND NOT unless within a term
                    c = "";
                    finishedTerm = true;
                }
                if (i == query.Length)
                {
                    // end of string
                    finishedTerm = true;
                }

                if (c == "+" && !inQuote)
                {
                    //ignore + signs
                    c = "";
                }

                term = term + c;
                if (finishedTerm)
                {
                    // finished with term

                    // double up double quotes for query
                    term = term.Replace("\"", "\"\"").Trim().ToLower();
                    if (!string.IsNullOrEmpty(term))
                    {
                        //does the term look like a document? if so, process it separately
                        bool blnNormalTerm = true;
                        if (blnNormalTerm) { }
                        // check we don't have to ignore this word
                        if (!termHadQuotes && (term.IndexOf("*") > -1 || term.IndexOf("?") > -1 || term.Trim().ToLower() == "or" || term.Trim().ToLower() == "and" || term.Trim().ToLower() == "not" || term.Trim().ToLower() == "near" || term.Trim().ToLower() == "-" || term.Trim().ToLower() == "+"))
                        {
                            aterms.Add(term.Trim().ToLower());
                        }
                        else
                        {
                            if (!noise.Contains(term.Trim()) && term.Length > 0)
                            {
                                // this is not a noise word
                                // but, a phrase may be made up of words that are all ignore and this will cause an error (e.g. "q&a")
                                int z = 0;
                                bool blnFoundValidWord = false;
                                bool gotWord = false;
                                string word = "";
                                word = "";
                                for (z = 1; z <= term.Length; z++)
                                {
                                    c = term.Substring(z - 1, 1);
                                    gotWord = false;
                                    if ("abcdefghijklmnopqrstuvwxyz0123456789".IndexOf(c) > -1)
                                    {
                                        // valid character
                                        word = word + c;
                                    }
                                    else
                                    {
                                        // punctuation
                                        gotWord = true;
                                    }
                                    if (z == term.Length)
                                    {
                                        gotWord = true;
                                    }
                                    if (gotWord)
                                    {
                                        if (word.Trim() == "")
                                        {
                                            gotWord = false;
                                        }
                                    }
                                    if (gotWord)
                                    {
                                        if (!noise.Contains(word))
                                        {
                                            // word is ok
                                            blnFoundValidWord = true;
                                            break;
                                        }
                                        word = "";
                                    }
                                }

                                if (blnFoundValidWord)
                                {
                                    aterms.Add((termHadQuotes ? "[quoted]" : "") + term);
                                };
                            }
                        }
                    }
                    term = "";
                    if (ccopy == "-" || ccopy == "(" || ccopy == ")")
                    {
                        aterms.Add(ccopy);
                    }
                    // prepare for next term
                    termHadQuotes = false;
                }
            }

            return aterms;
        }

        private static string GetMultiFieldWHEREInner(List<string> terms, List<string> fieldNames, ref int termPointer)
        {
            var where = new StringBuilder();
            var expression = new StringBuilder();

            bool negated = false;

            string op = "";

            while (termPointer < terms.Count)
            {
                var term = terms[termPointer];
                termPointer++;

                switch (term.Trim().ToLower())
                {
                    //case "near":
                    //    expression += " NEAR ";
                    //    break;
                    case "and":
                    case "+":
                        op = "AND";
                        break;
                    case "or":
                        op = "OR";
                        break;
                    case "not":
                    case "-":
                        negated = true;
                        break;
                    case "(":
                        // going into new level for expression
                        var nestedClause = GetMultiFieldWHEREInner(terms, fieldNames, ref termPointer);

                        if (negated)
                        {
                            nestedClause = $"!({nestedClause})";
                        }

                        switch (op)
                        {
                            case "AND":
                                where.Append($" & ({nestedClause})");
                                break;
                            case "OR":
                                where.Append($" || ({nestedClause})");
                                break;
                        }

                        // reset for next term
                        negated = false;
                        expression = new StringBuilder();
                        op = "";

                        break;
                    case ")":
                        // finished with this level

                        return where.ToString();
                        break;
                    default:
                        // got a term
                        var isQuoted = term.EndsWith("[quoted]");
                        term = term.Replace("[quoted]", "");

                        var clause = new StringBuilder();
                        clause.Append("(");
                        foreach (var fieldName in fieldNames)
                        {
                            if (clause.Length > 1)
                            {
                                clause.Append(" OR ");
                            }
                            if (isQuoted)
                            {
                                clause.Append($"{fieldName} = \"{term}\"");
                            }
                            else
                            {
                                clause.Append($"{fieldName}.Contains(\"{term}\")");
                            }
                        }
                        clause.Append(")");

                        if (negated)
                        {
                            clause.Insert(0, "!");
                        }

                        if (where.Length == 0)
                        {
                            where.Append(clause);
                        }
                        else
                        {
                            switch (op)
                            {
                                case "OR":
                                    where.Append($" OR {clause}");
                                    break;
                                default:
                                    where.Append($" AND {clause}");
                                    break;
                            }
                        }

                        // reset for next term
                        negated = false;
                        expression = new StringBuilder();
                        op = "";

                        break;
                }
            }

            return where.ToString();
        }
    }
}
