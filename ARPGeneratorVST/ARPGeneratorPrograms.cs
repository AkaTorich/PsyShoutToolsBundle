// ARPGeneratorPrograms.cs - Пресеты плагина
using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace ARPGeneratorVST
{
    internal sealed class ARPGeneratorPrograms : VstPluginProgramsBase
    {
        protected override VstProgramCollection CreateProgramCollection()
        {
            var programs = new VstProgramCollection();

            var program1 = new VstProgram();
            program1.Name = "C Major Arp";
            programs.Add(program1);

            var program2 = new VstProgram();
            program2.Name = "A Minor Arp";
            programs.Add(program2);

            var program3 = new VstProgram();
            program3.Name = "G Dorian Arp";
            programs.Add(program3);

            return programs;
        }
    }
}

