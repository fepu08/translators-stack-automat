using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxAnalysisWithSymbolTableWPF
{
    static class ExtensionMethods
    {
        public static Stack<T> Clone<T>(this Stack<T> stack)
        {
            Contract.Requires(stack != null);
            return new Stack<T>(new Stack<T>(stack));
        }
    }
}
