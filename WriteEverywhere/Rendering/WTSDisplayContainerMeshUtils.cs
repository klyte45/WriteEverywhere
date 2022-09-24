using UnityEngine;

namespace WriteEverywhere.Rendering
{
    internal static class WTSDisplayContainerMeshUtils
    {
        public const float FRONT_HEIGHT_BASE = 1f;
        public const float BACK_HEIGHT_BASE = 1f;
        public const float FRONT_WIDTH_BASE = 1f;
        public const float BACK_WIDTH_BASE = 1f;
        public const float DEPTH_BASE = 0.5f;
        public const float FRONT_BORDER_BASE = 0.05f;

        private const float offsetX = 0f;
        public static void GenerateDisplayContainer(Vector2 frontWH, Vector2 backWH, Vector2 backCenterOffset, float frontDepth, float backDepth, float frontBorderThickness, out Vector3[] points)
        {
            /**
             * 
             *  P0.________________________________________________.P1
             *    |  I0________________________________________I1  |
             *    |    |                                      |    |
             *    |    |                                      |    |
             *    |    |                                      |    |
             *    |    |                                      |    |
             *    |    |                                      |    |
             *    |  I2|______________________________________|I3  |
             *    |________________________________________________|
             *  P2                                                  P3
             *         As0._____ 
             *          _/     |Ps0
             *        _/       |
             *      _/         |     
             *  Qs0/           |    Q1.____________________________________________.Q0
             *     |           |      |                                            |
             *     |           |      |                                            |       
             *     |           |      |                                            |
             *     |___________|      |____________________________________________|
             *  Qs2    As2     Ps2  Q3                                             Q2
             *  
             *  P0-P1 = A0-A1 = frontWH.x
             *  P0-P2 = A0-A2 = frontWH.y 
             *  Q0-Q1 = backWH.x
             *  Q0-Q2 = backWH.y  
             *  
             *  (A0-A1)/2 - (Q0-Q1)/2 = backCenterOffset.x
             *  (A0-A2)/2 - (Q0-Q2)/2 = backCenterOffset.y
             *  
             *  (P0_P2) - (A0_A2) = frontDepth
             *  (Q0_Q2) - (A0_A2) = backDepth
             *  (I0-P0)² = frontBorderThickness  
             *  
             *  point | index
             *  =============
             *  I0    |   0
             *  I1    |   1
             *  I2    |   2
             *  I3    |   3
             *  P0    |   4
             *  P1    |   5
             *  P2    |   6
             *  P3    |   7             
             *  
             *  Q0    |   8
             *  Q1    |   9     
             *  Q2    |  10
             *  Q3    |  11 
             *  
             *  Aps0   |  12
             *  Aps1   |  13     
             *  Aps2   |  14
             *  Aps3   |  15  
             *  Ps0   |  16
             *  Ps1   |  17
             *  Ps2   |  18
             *  Ps3   |  19
             *  Qs0   |  20
             *  Qs1   |  21     
             *  Qs2   |  22
             *  Qs3   |  23                
             *  
             *  Apz0   |  24
             *  Apz1   |  25  
             *  Apz2   |  26
             *  Apz3   |  27 
             *  Pz0   |  28
             *  Pz1   |  29
             *  Pz2   |  30
             *  Pz3   |  31
             *  Qz0   |  32
             *  Qz1   |  33    
             *  Qz2   |  34
             *  Qz3   |  35 
             *  
             *  Aqs0   | 36
             *  Aqs1   | 37   
             *  Aqs2   | 38
             *  Aqs3   | 39
             *  Aqz0   | 40
             *  Aqz1   | 41   
             *  Aqz2   | 42
             *  Aqz3   | 43
             */

            var I0 = new Vector3(-frontWH.x * .5f + frontBorderThickness, (frontWH.y * .5f - frontBorderThickness), frontDepth); //I0
            var I1 = new Vector3(frontWH.x * .5f - frontBorderThickness, (frontWH.y * .5f - frontBorderThickness), frontDepth);  //I1
            var I2 = new Vector3(-frontWH.x * .5f + frontBorderThickness - offsetX, (frontWH.y * -.5f + frontBorderThickness), frontDepth);//I2
            var I3 = new Vector3(frontWH.x * .5f - frontBorderThickness + offsetX, (frontWH.y * -.5f + frontBorderThickness), frontDepth); //I3                
            var P0 = new Vector3(-frontWH.x * .5f, frontWH.y * .5f, frontDepth); //P0
            var P1 = new Vector3(frontWH.x * .5f, frontWH.y * .5f, frontDepth);  //P1
            var P2 = new Vector3(-frontWH.x * .5f - offsetX, frontWH.y * -.5f, frontDepth);//P2
            var P3 = new Vector3(frontWH.x * .5f + offsetX, frontWH.y * -.5f, frontDepth); //P3
            var Q0 = new Vector3(-backWH.x * .5f, backWH.y * .5f, -backDepth) + (Vector3)backCenterOffset; //Q0
            var Q1 = new Vector3(backWH.x * .5f, backWH.y * .5f, -backDepth) + (Vector3)backCenterOffset; //Q1
            var Q2 = new Vector3(-backWH.x * .5f - offsetX, backWH.y * -.5f, -backDepth) + (Vector3)backCenterOffset;//Q2
            var Q3 = new Vector3(backWH.x * .5f + offsetX, backWH.y * -.5f, -backDepth) + (Vector3)backCenterOffset; //Q3
            var A0 = new Vector3(-frontWH.x * .5f, frontWH.y * .5f, 0); //A0
            var A1 = new Vector3(frontWH.x * .5f, frontWH.y * .5f, 0);  //A1
            var A2 = new Vector3(-frontWH.x * .5f - offsetX, frontWH.y * -.5f, 0);//A2
            var A3 = new Vector3(frontWH.x * .5f + offsetX, frontWH.y * -.5f, 0); //A3

            points = new Vector3[]
            {
               I0 ,
               I1 ,
               I2 ,
               I3 ,
               P0 ,
               P1 ,
               P2 ,
               P3 ,

               Q0 ,
               Q1 ,
               Q2 ,
               Q3 ,

               A0 ,
               A1 ,
               A2 ,
               A3 ,
               P0 ,
               P1 ,
               P2 ,
               P3 ,
               Q0 ,
               Q1 ,
               Q2 ,
               Q3 ,

               A0 ,
               A1 ,
               A2 ,
               A3 ,
               P0 ,
               P1 ,
               P2 ,
               P3 ,
               Q0 ,
               Q1 ,
               Q2 ,
               Q3,


               A0 ,
               A1 ,
               A2 ,
               A3 ,
               A0 ,
               A1 ,
               A2 ,
               A3 ,
            };

        }

        public static int[] m_triangles = new int[]
        {
            // FRONT
            4,0,5, // P0-I0-P1
            0,1,5, // I0-I1-P1
            5,1,7, // P1-I1-P3
            1,3,7, // I1-I3-P3
            3,2,7, // I3-I2-P3
            2,6,7, // I2-P2-P3
            0,6,2, // I0-P2-I2
            4,6,0, // P0-P2-I0


            8,9,10, // Q0-Q1-Q2
            9,11,10, // Q1-Q3-Q2


            38,36,20, // Aqs2-Aqs0-Qs0
            38,20,22, // Aqs2-Qs0-Qs2
            18,16,12, // Ps2-Ps0-Aps0
            18,12,14, // Ps2-Aps0-Aps2


            39,21,37, // Aqs3-Qs1-Aqs1
            39,23,21, // Aqs3-Qs3-Qs1
            19,13,17, // Ps3-Aps1-Ps1
            19,15,13, // Ps3-Aps3-Aps1


            41,32,40, // Aqz1-Qz0-Aqz0
            41,33,32, // Aqz1-Qz1-Qz0
            29,40,28, // Pz1-Aqz0-Pz0
            29,41,40, // Pz1-Aqz1-Aqz0


            34,43,42, // Qz2-Aqz3-Aqz2
            35,43,34, // Qz3-Aqz3-Qz2
            26,31,30, // Apz2-Pz3-Pz2
            27,31,26, // Apz3-Pz3-Apz2

        };

        public static readonly int[] m_trianglesGlass = new int[]
        {
             0,2,1,    // I0-I2-I1
             3,1,2,    // I3-I1-I2
        };
    }


}




































