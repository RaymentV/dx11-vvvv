﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using VVVV.DX11.Lib;
namespace VVVV.DX11.Nodes
{
    public abstract class AbstractDX11LayerSpaceNode : IPluginEvaluate, IDX11LayerHost
    {


        [Config("Apply Per Slice")]
        protected ISpread<bool> perSlice;

        [Input("Layer In")]
        protected Pin<DX11Resource<DX11Layer>> FLayerIn;

        [Input("Enabled",DefaultValue=1, Order = 100000)]
        protected IDiffSpread<bool> FEnabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> FOutLayer;

        private DX11SingleSliceOrder getSliceOrder = new DX11SingleSliceOrder();

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutLayer[0] == null) { this.FOutLayer[0] = new DX11Resource<DX11Layer>(); }
        }


        #region IDX11ResourceProvider Members

        public void Update(DX11RenderContext context)
        {
            if (!this.FOutLayer[0].Contains(context))
            {
                this.FOutLayer[0][context] = new DX11Layer();
                this.FOutLayer[0][context].Render = this.Render;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutLayer.SafeDisposeAll(context);
        }

        public void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.FLayerIn.SliceCount == 0) { return; }

            int layerCount = this.LayerCount;
            if (layerCount == 0)
                return;

            if (this.FEnabled[0])
            {
                if (this.FLayerIn.IsConnected)
                {
                    Matrix view = settings.View;
                    Matrix projection = settings.Projection;
                    Matrix vp = settings.ViewProjection;
                    Matrix crop = settings.Crop;
                    Matrix aspect = settings.Aspect;
                    Matrix rawProj = settings.RawProjection;
                    bool depthonly = settings.DepthOnly;

                    if (this.perSlice[0])
                    {
                        IDX11LayerOrder currentOrder = settings.LayerOrder;
                        settings.LayerOrder = getSliceOrder;
                    

                        //Add layer pin to spreadmax contrib
                        layerCount = Math.Max(layerCount, this.FLayerIn.SliceCount);

                        for (int i = 0; i < layerCount; i++)
                        {
                            getSliceOrder.FInIndex = i;

                            this.UpdateSettings(settings, i);

                            this.FLayerIn.RenderSlice(context, settings, i);

                            //Restore slice
                            settings.View = view;
                            settings.Projection = projection;
                            settings.ViewProjection = vp;
                            settings.DepthOnly = depthonly;
                            settings.Crop = crop;
                            settings.Aspect = aspect;
                            settings.RawProjection = rawProj;
                        }

                        settings.LayerOrder = currentOrder;

                    }
                    else
                    {
                        for (int i = 0; i < layerCount; i++)
                        {
                            this.UpdateSettings(settings, i);

                            this.FLayerIn.RenderAll(context, settings);
                        }

                        settings.View = view;
                        settings.Projection = projection;
                        settings.ViewProjection = vp;
                        settings.DepthOnly = depthonly;
                        settings.Crop = crop;
                        settings.Aspect = aspect;
                        settings.RawProjection = rawProj;
                    }
                }
            }
            else
            {
                this.FLayerIn.RenderAll(context, settings);
            }
        }

        protected abstract int LayerCount
        {
            get;
        }


        protected abstract void UpdateSettings(DX11RenderSettings settings, int slice);


        #endregion
    }




}
