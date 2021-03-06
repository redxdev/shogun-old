﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sholan.Compiler.Nodes
{
    public class PlaceholderNode : AbstractCompileNode
    {
        public PlaceholderNode(int line, int col)
            : base(line, col)
        {
            this.Attributes
                .Has("placeholder");
        }

        public override void PrePass(Kernel k)
        {
            base.PrePass(k);
        }

        public override void PreCompile(Kernel k)
        {
        }

        public override void Compile(Kernel k)
        {
        }
    }
}
