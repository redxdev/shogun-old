﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using sholan.Language;

namespace sholan.Compiler
{
    public class Kernel
    {
        private Stack<Scope> scopeStack = new Stack<Scope>();

        private List<Operation> operations = new List<Operation>();

        private bool hasEntry = false;

        private HashSet<string> importedFiles = new HashSet<string>();

        public Scope CurrentScope
        {
            get
            {
                return scopeStack.Peek();
            }
        }

        public List<Operation> Operations
        {
            get
            {
                return this.operations;
            }
        }

        public bool HasEntry
        {
            get
            {
                return this.hasEntry;
            }

            set
            {
                if (value && this.hasEntry)
                    throw new CompileException("Kernel already has entry point");

                this.hasEntry = value;
            }
        }

        public Kernel()
        {
            PushScope().MemorySpace = 1;
        }

        public Scope PushScope()
        {
            Scope scope = new Scope();
            if(scopeStack.Count > 0)
                scope.Parent = scopeStack.Peek();

            scopeStack.Push(scope);
            return scope;
        }

        public Scope PopScope()
        {
            return scopeStack.Pop();
        }

        public Symbol RegisterSymbol(Symbol symbol)
        {
            symbol.SScope = this.CurrentScope;
            this.CurrentScope.Symbols.Add(symbol.Name, symbol);
            return symbol;
        }

        public string GetScopeName()
        {
            string result = string.Empty;
            foreach (Scope scope in scopeStack.Reverse())
            {
                result += scope.Name + "_";
            }

            if (scopeStack.Count > 0)
                result = result.Substring(0, result.Length - 1);

            return result;
        }

        public Symbol Lookup(string symbolName)
        {
            foreach(Scope scope in scopeStack)
            {
                if(scope.Symbols.ContainsKey(symbolName))
                {
                    return scope.Symbols[symbolName];
                }
            }

            throw new KeyNotFoundException(string.Format("Unknown symbol \"{0}\"", symbolName));
        }

        public Operation Emit(Opcode o)
        {
            Operation op = new Operation()
                {
                    Op = o,
                    Argument = string.Empty
                };
            this.operations.Add(op);
            return op;
        }

        public Operation Emit(Opcode o, string argument)
        {
            Operation op = new Operation()
            {
                Op = o,
                Argument = argument
            };
            this.operations.Add(op);
            return op;
        }

        public Operation Emit(Operation o)
        {
            this.operations.Add(o);
            return o;
        }

        public Operation EmitPush(string argument)
        {
            Operation op = new Operation()
            {
                Op = Opcode.PUSH,
                Argument = argument
            };
            this.operations.Add(op);
            return op;
        }

        public void Import(string file)
        {
            string fullPath = Path.GetFullPath(file);
            if (importedFiles.Contains(fullPath))
                return;

            try
            {
                Nodes.ICompileNode root = LanguageUtilities.ParseFile(fullPath);
                this.Compile(root, true);
            }
            catch(Exception e)
            {
                throw new CompileException("Encountered exception while including " + fullPath, e);
            }
        }

        public void Compile(Nodes.ICompileNode root, bool imported = false)
        {
            root.PrePass(this);
            root.PreCompile(this);

            if (!imported)
            {
                if (!this.HasEntry)
                {
                    throw new CompileException("No entry point found");
                }

                this.Emit(Opcode.GOTO, "\"sl_k_entry\"");
            }

            root.Compile(this);
        }

        public void EndCompile(bool halt = false)
        {
            if(halt)
                this.Emit(Opcode.HALT).Comment = "end compile (halt=true)";
        }

        public void Write(string file)
        {
            using(StreamWriter writer = new StreamWriter(file))
            {
                writer.Write("; Generated by sholan compiler" + Environment.NewLine);

                foreach(Operation op in this.operations)
                {
                    writer.Write(op.GetOutput() + Environment.NewLine);
                }
            }
        }
    }
}
