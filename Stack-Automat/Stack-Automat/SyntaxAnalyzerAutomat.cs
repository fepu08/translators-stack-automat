using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyntaxAnalysisWithSymbolTableWPF
{
    class SyntaxAnalyzerAutomat
    {
        private string original;
        private string converted;

        private int readingHeadIndex;

        private Stack<string> restOfInput;
        private Stack<string> methods;
        private List<string> steps;

        private List<string> solutionSteps;
        private string[][] table;
        private static string accept = "accept";

        public string RestOfInput { get; }

        public string[][] Table { get; set; }

        public int ReadingHeadIndex { get; set; }

        public string Original
        {
            get { return original; }
            set 
            { 
                original = value;
                this.converted = Simple(original);
                if (this.converted.Substring(  this.converted.Length - 1) != "#") { this.converted += "#"; }
                this.restOfInput = InitRestOfInput(converted);
                readingHeadIndex = 0;
            }
        }

        public string Converted 
        {
            get { return converted; }
            set 
            {
                converted = value;
                if(converted.Substring(converted.Length - 1) != "#") converted += "#";
                this.restOfInput = InitRestOfInput(Converted);
                readingHeadIndex = 0;
            }
        }

        public Stack<string> Methods
        {
            get
            {
                return new Stack<string>(new Stack<string>(methods));
            }
            set
            {
                this.methods = value;
            }
        }

        public List<string> Steps 
        {
            get
            {
                List<string> temp = new List<string>();
                for (int i = 0; i < steps.Count; i++)
                {
                    temp.Add(steps[i]);
                }
                return temp;
            }
            set { steps = value; } 
        }

        public List<string> SolutionSteps 
        {
            get
            {
                List<string> temp = new List<string>();
                for (int i = 0; i < solutionSteps.Count; i++)
                {
                    temp.Add(solutionSteps[i]);
                }
                return temp;
            }
            set { solutionSteps = value; }
        }

        public SyntaxAnalyzerAutomat()
        {
            this.readingHeadIndex = 0;
            this.original = "";
            this.converted = "";
            this.restOfInput = new Stack<string>();
            this.methods = new Stack<string>();
            methods.Push("#");
            methods.Push("E");
            this.steps = new List<string>();
            this.solutionSteps = new List<string>();
        }

        public SyntaxAnalyzerAutomat(string original) : this()
        {
            this.readingHeadIndex = 0;
            this.original = original;
            this.converted = Simple(original);
            if(this.converted.Substring(this.converted.Length - 1) != "#") { this.converted += "#"; }
            this.restOfInput = InitRestOfInput(converted);
        }

        public string GetSolution()
        {
            return String.Format("({0}, {1}, {2})", StackToString(restOfInput), StackToString(methods), StepsToString());
        }

        public string Simple(string input)
        {
            // Az összes számjegyet kicseréli i betűre
            return Regex.Replace(input, "[0-9]+", "i");
        }

        private string StepsToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < steps.Count; i++)
            {
                sb.Append(steps[i]);
            }
            return sb.ToString();
        }

        private string StackToString(Stack<string> stack)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string word in stack)
            {
                sb.Append(word.ToString());
            }
            return sb.ToString();
        }

        public void ReadTable(String path, Char separator)
        {
            StreamReader sr = new StreamReader(path);
            var lines = new List<string[]>();
            int Row = 0;
            while (!sr.EndOfStream)
            {
                string[] Line = sr.ReadLine().Split(separator);
                lines.Add(Line);
                Row++;
                Console.WriteLine(Row);
            }

            var data = lines.ToArray();
            table = data;
        }

        private Stack<string> InitRestOfInput(string input)
        {
            Stack<string> result = new Stack<string>();
            for (int i = input.Length - 1; i >= 0; i--)
            {
                result.Push(input[i].ToString());
            }
            return result;
        }

        public bool Solve()
        {
            int actualInputCharacterIndex;
            int actualMethodIndex;
            string[] helper = new string[2];
            while (restOfInput.Count > 0)
            {
                actualInputCharacterIndex = GetIndexOfActual(readingHeadIndex);
                if (actualInputCharacterIndex == -1) return false;

                actualMethodIndex = GetActualMethodIndex();
                if (actualMethodIndex == -1) return false;

                string nextMethod = GetNextMethod(actualMethodIndex, actualInputCharacterIndex);
                // ha üres, hibát találtunk
                if (nextMethod.Equals("")) return false;

                // ha az accept szót találjuk akkor sikeres
                if (nextMethod.Equals(accept)) return true;

                // ha pop szó, akkor ki kell venni a verem tetején lévő elemet ...
                if (nextMethod.Equals("pop"))
                {
                    restOfInput.Pop();
                    readingHeadIndex++;
                }
                else
                {
                    nextMethod = nextMethod.Substring(1, nextMethod.Length - 2);
                    helper = nextMethod.Split(',');
                    steps.Add(helper[1]);
                    if (helper[0] == "eps")
                    {
                        solutionSteps.Add(GetSolution());
                        continue;
                    }
                    for (int i = 0; i < helper[0].Length; i++)
                    {
                        string last = helper[0].Substring(helper[0].Length - (i + 1), 1);
                        if (last == "'")
                        {
                            i++;
                            methods.Push(helper[0].Substring(helper[0].Length - (i + 1), 2));
                        }
                        else { methods.Push(last); }
                    }
                }
                solutionSteps.Add(GetSolution());
            }
            return false;
        }

        private int FindOnInput(string actualOnInput)
        {
            //TODO create exception
            if (table == null) throw new NullReferenceException();
            for (int i = 0; i < table[0].Length; i++)
            {
                if (table[0][i] == actualOnInput) return i;
            }
            return -1;
        }

        private int FindAmongMethods(string actualMethod)
        {
            //TODO create exception
            if (table == null) throw new NullReferenceException();
            for (int i = 1; i < table.Length; i++)
            {
                if (table[i][0] == actualMethod) return i;
            }
            return -1;
        }

        private int GetIndexOfActual(int actualInputIndex)
        {
            string actualOnInput = converted.Substring(actualInputIndex, 1); //legfelső sorban kell keresni
            int actualInputCharacterIndex = FindOnInput(actualOnInput);
            return actualInputCharacterIndex;
        }

        private int GetActualMethodIndex()
        {
            string actualMethod = methods.Pop(); // a következő sorok 0. indexében kell keresni (jelöljük most b-vel)
            int indexOfMethod = FindAmongMethods(actualMethod);
            return indexOfMethod;
        }

        private string GetNextMethod(int rowIndex, int columnIndex)
        {
            return table[rowIndex][columnIndex];
        }

        

    }
}
