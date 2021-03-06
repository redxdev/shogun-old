﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sholan.Compiler.Nodes
{
    public class FunctionCallNode : AbstractCompileNode
    {
        public string Function
        {
            get;
            set;
        }

        public List<ICompileNode> Arguments
        {
            get;
            set;
        }

        public FunctionCallNode(int line, int col)
            : base(line, col)
        {
            this.Attributes
                .Has("function-call")
                .Has("value");
        }

        public override void PrePass(Kernel k)
        {
            base.PrePass(k);

            foreach (ICompileNode node in this.Arguments)
            {
                node.Attributes
                    .Check("value");
            }

            foreach (ICompileNode node in this.Arguments)
            {
                node.PrePass(k);
            }
        }

        public override void PreCompile(Kernel k)
        {
            foreach (ICompileNode node in this.Arguments)
            {
                node.PreCompile(k);
            }
        }

        public override void Compile(Kernel k)
        {
            Symbol symbol = k.Lookup(this.Function);

            switch(symbol.SType)
            {
                case Symbol.Type.Function:
                    break;

                default:
                    throw new InvalidOperationException(string.Format("Tried to compile a function call on a non-function symbol \"{0}\"", this.Function));
            }

            if (symbol.Args != this.Arguments.Count)
                throw new ArgumentOutOfRangeException(string.Format("Tried to pass {0} arguments to {1} (requires: {2})", this.Arguments.Count, symbol.Name, symbol.Args));

            switch(symbol.SMode)
            {
                default:
                    throw new InvalidOperationException(string.Format("Unknown symbol mode {0} on symbol \"{1}\"", symbol.SMode.ToString(), this.Function));

                case Symbol.Mode.Extern:
                    this.CompileExtern(k);
                    break;

                case Symbol.Mode.Intern:
                    this.CompileIntern(k);
                    break;

                case Symbol.Mode.Library:
                    this.CompileLibrary(k);
                    break;
            }

            if(!this.UseReturn)
            {
                k.Emit(Opcode.POP).Comment = "remove unused return value";
            }
        }

        protected void CompileExtern(Kernel k)
        {
            for(int i = this.Arguments.Count - 1; i >= 0; i--)
            {
                this.Arguments[i].Compile(k);
            }

            k.EmitPush('"' + this.Function + '"');
            k.Emit(Opcode.ECALL).SetDebug(File, Line, Column, DebugType.ECall, this.Function);
        }

        protected void CompileIntern(Kernel k)
        {
            for(int i = this.Arguments.Count - 1; i >= 0; i--)
            {
                this.Arguments[i].Compile(k);
            }

            uint returnId = k.CurrentScope.RequestLabelId();

            k.CurrentScope.PushMemory(k);
            k.Emit(Opcode.PLABL, "\"sl_r_" + k.GetScopeName() + "_" + returnId.ToString() + "\"");
            k.Emit(Opcode.GOTO, '"' + k.Lookup(this.Function).AsmName + '"').SetDebug(File, Line, Column, DebugType.Call, this.Function);

            k.Emit(Opcode.LABEL, "sl_r_" + k.GetScopeName() + "_" + returnId.ToString()).Comment = "return point from " + this.Function;
            k.CurrentScope.PopMemory(k);
        }

        protected void CompileLibrary(Kernel k)
        {
            for (int i = this.Arguments.Count - 1; i >= 0; i--)
            {
                this.Arguments[i].Compile(k);
            }

            uint returnId = k.CurrentScope.RequestLabelId();

            k.Emit(Opcode.PLABL, "\"sl_r_" + k.GetScopeName() + "_" + returnId.ToString() + "\"");
            var rvn = new RetrieveVariableNode(-1, -1)
                {
                    VariableName = this.Function
                };
            rvn.PrePass(k);
            rvn.PreCompile(k);
            rvn.Compile(k);
            k.CurrentScope.PushMemory(k);
            k.Emit(Opcode.JUMP).SetDebug(File, Line, Column, DebugType.LCall, this.Function);

            k.Emit(Opcode.LABEL, "sl_r_" + k.GetScopeName() + "_" + returnId.ToString()).Comment = "return point from " + this.Function;
            k.CurrentScope.PopMemory(k);
        }
    }
}
