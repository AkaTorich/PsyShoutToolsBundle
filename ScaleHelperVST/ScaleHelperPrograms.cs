// ScaleHelperPrograms.cs - Пресеты плагина
using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace ScaleHelperVST
{
    internal sealed class ScaleHelperPrograms : VstPluginProgramsBase
    {
        protected override VstProgramCollection CreateProgramCollection()
        {
            var programs = new VstProgramCollection();

            var program1 = new VstProgram();
            program1.Name = "C Major";
            programs.Add(program1);

            var program2 = new VstProgram();
            program2.Name = "A Minor";
            programs.Add(program2);

            var program3 = new VstProgram();
            program3.Name = "G Major";
            programs.Add(program3);

            return programs;
        }
    }
}