﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sholan.Compiler.Nodes
{
    class ConstantBoolNode : AbstractCompileNode
    {
        public bool Value
        {
            get;
            set;
        }

        public ConstantBoolNode()
            : base()
        {
            this.Attributes
                .Has("value")
                .Has("constant");
        }

        public override void PrePass(Kernel k)
        {
        }

        public override void PreCompile(Kernel k)
        {
        }

        public override void Compile(Kernel k)
        {
            if(this.Value)
            {
                k.Emit(Opcode.PUSHT);
            }
            else
            {
                k.Emit(Opcode.PUSHF);
            }
        }
    }
}
