﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sholan.Compiler.Nodes
{
    public class IfNode : AbstractCompileNode
    {
        public ICompileNode Check
        {
            get;
            set;
        }

        public ICompileNode BranchTrue
        {
            get;
            set;
        }

        public ICompileNode BranchFalse
        {
            get;
            set;
        }

        public IfNode()
            : base()
        {
            this.Attributes
                .Has("if");
        }

        public override void PrePass(Kernel k)
        {
            this.Check.Attributes
                .Check("value");


            this.Check.PrePass(k);

            if(this.BranchTrue != null)
                this.BranchTrue.PrePass(k);

            if (this.BranchFalse != null)
                this.BranchFalse.PrePass(k);
        }

        public override void PreCompile(Kernel k)
        {
            this.Check.PreCompile(k);

            if (this.BranchTrue != null)
                this.BranchTrue.PreCompile(k);

            if (this.BranchFalse != null)
                this.BranchFalse.PreCompile(k);
        }

        public override void Compile(Kernel k)
        {
            k.CurrentScope.PushMemory(k);

            Scope ifScope = k.PushScope();
            ifScope.Name = "if" + ifScope.Parent.RequestLabelId();

            string trueLabel = "sl_if_" + k.GetScopeName();
            string falseLabel = "sl_fe_" + k.GetScopeName();
            string endLabel = "sl_fh_" + k.GetScopeName();

            this.Check.Compile(k);

            k.Emit(Opcode.GOTOF, '"' + trueLabel + '"').Comment = "if statement";
            k.Emit(Opcode.GOTO, '"' + falseLabel + '"');

            k.Emit(Opcode.LABEL, trueLabel);

            k.CurrentScope.PushMemory(k);

            Scope trueScope = k.PushScope();
            trueScope.Name = "true";

            if (this.BranchTrue != null)
                this.BranchTrue.Compile(k);

            k.PopScope();

            k.CurrentScope.PopMemory(k);

            if(this.BranchFalse == null)
            {
                k.Emit(Opcode.LABEL, falseLabel).Comment = "no else block, used as end label";
            }
            else if(this.BranchFalse != null)
            {
                k.Emit(Opcode.GOTO, '"' + endLabel + '"');

                k.Emit(Opcode.LABEL, falseLabel);

                k.CurrentScope.PushMemory(k);

                Scope falseScope = k.PushScope();
                falseScope.Name = "false";

                this.BranchFalse.Compile(k);

                k.PopScope();

                k.CurrentScope.PopMemory(k);

                k.Emit(Opcode.LABEL, endLabel).Comment = "end if";
            }

            k.PopScope();

            k.CurrentScope.PopMemory(k);
        }
    }
}
