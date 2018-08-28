﻿using System.Collections.Generic;
using VVVV.DX11.Validators;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "WithinViewportIndex", Category = "DX11.Validator", Version = "", Author = "velcrome", Tags = "layer, slice, routing")]
    public class WithinViewportIndexNode : IPluginEvaluate
    {
        [Input("Enabled", DefaultValue = 1, IsSingle = true)]
        protected ISpread<bool> FInEnabled;

        [Input("Viewport Index", DefaultValue = 0)]
        protected ISpread<int> FViewportIndexList;

        [Output("Output", IsSingle = true)]
        protected ISpread<DX11WithinViewportValidator> FOut;

        public void Evaluate(int SpreadMax)
        {
            if (this.FOut[0] == null) { this.FOut[0] = new DX11WithinViewportValidator(); }

            if (!FInEnabled.IsChanged || !FViewportIndexList.IsChanged) return;

            this.FOut[0].Enabled = this.FInEnabled[0];
            this.FOut[0].ViewPortIndices = new List<int>(FViewportIndexList);
            this.FOut[0].Reset();
        }
    }
}
