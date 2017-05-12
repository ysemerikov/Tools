﻿using System.Linq;
using Tools.SerializerComparer.Models;

namespace Tools.SerializerComparer
{
    public class EntryPoint
    {
        public static void Start()
        {
//            PerformanceTester<SmallModel>.Start(SmallModel.Generate(32*1024).ToList(), 6, false);
            PerformanceTester<BigModel>.Start(BigModel.Generate(1024).ToList(), 6, false);
//            PerformanceTester<NormalModel>.Start(NormalModel.Generate(16*1024).ToList(), 6, false);
        }
    }
}