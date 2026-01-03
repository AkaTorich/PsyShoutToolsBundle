// PluginCommandStub.cs - Точка входа для VST
using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace ScaleHelperVST
{
    /// <summary>
    /// Точка входа для VST плагина
    /// </summary>
    public sealed class PluginCommandStub : StdPluginCommandStub
    {
        /// <summary>
        /// Создаёт экземпляр плагина
        /// </summary>
        protected override IVstPlugin CreatePluginInstance()
        {
            return new ScaleHelperVstPlugin();
        }
    }
}
