﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldLoader
{
    public static class Hashes
    {
        //these are generated from hk's assets files
        //
        //assetstools has a hash function that should generate them...
        //they match up with files that have type tree info,
        //but don't with files that don't have type tree info
        public static Dictionary<int, uint[]> hashes = new Dictionary<int, uint[]> {
            {1,new uint[]{1751413411u,649051188u,466792938u,1402331945u}},
            {4,new uint[]{531111030u,1108691320u,4164410554u,1429515024u}},
            {20,new uint[]{2111970198u,453760530u,488186164u,1990743634u}},
            {23,new uint[]{1765508783u,733526377u,84887017u,1704932212u}},
            {33,new uint[]{2972718749u,616358871u,1483662507u,2040636000u}},
            {81,new uint[]{3930742320u,357283531u,3658491270u,3405936084u}},
            {82,new uint[]{1625460746u,1122470541u,1870920130u,3433923685u}},
            {92,new uint[]{2299845344u,2484852737u,3613891620u,3332171101u}},
            {95,new uint[]{2468988612u,592190103u,2307894843u,2622534740u}},
            {104,new uint[]{714583296u,3135344027u,3798125546u,1757006743u}},
            {157,new uint[]{156710160u,1597347260u,325666391u,185435564u}},
            {198,new uint[]{497729357u,1457220335u,67655067u,88316849u}},
            {199,new uint[]{2971396826u,979478203u,28323067u,3567292362u}},
            {212,new uint[]{1185728132u,151226863u,1069271953u,2301107666u}},
            {222,new uint[]{3552053918u,2586112918u,2480756793u,469139727u}},
            {223,new uint[]{2049771736u,4188234038u,984154314u,2238013537u}},
            {224,new uint[]{2164721022u,3517549776u,3947611915u,3049492862u}},
            {225,new uint[]{2469119067u,2858914325u,2787420091u,53908135u}},
            {114,new uint[]{3935837163u,1008504293u,1173346013u,1107710547u}},
            {21,new uint[]{1832153560u,700189593u,3287548493u,1830535014u}},
            {50,new uint[]{174558062u,2558437158u,3138479896u,1416336390u}},
            {58,new uint[]{2941728903u,3679078761u,453211099u,65824931u}},
            {61,new uint[]{1846366768u,136435456u,3472494665u,4049682847u}},
            {102,new uint[]{1710827166u,3360805777u,3037850079u,9446364u}},
            {43,new uint[]{302509207u,2738760208u,177000433u,1891649358u}},
            {60,new uint[]{432782770u,891175161u,2905560776u,1480734674u}},
            {64,new uint[]{605288100u,3676966350u,430280629u,1349669865u}},
            {68,new uint[]{1550815777u,818613044u,2915734078u,2099143000u}},
            {96,new uint[]{2927594007u,1665722046u,4242764691u,1112349808u}},
            {108,new uint[]{71493193u,3551303937u,559289618u,1479365138u}},
            {258,new uint[]{1662855981u,3983026923u,4126653332u,2065547791u}},
            {132,new uint[]{373056807u,3112088u,2248271863u,3111696653u}},
            {150,new uint[]{1437606897u,2113543590u,2362931654u,2211704026u}},
            {28,new uint[]{769165086u,1347997775u,1719155866u,1450381019u}},
            {48,new uint[]{290680102u,722314047u,1747423109u,346697465u}},
            {74,new uint[]{896828609u,1416566816u,551420000u,915919348u}},
            {83,new uint[]{1179202738u,3714266974u,2287612697u,2578319850u}},
            {91,new uint[]{100584449u,4291296198u,2952322051u,4114555304u}},
            {128,new uint[]{147540913u,455959397u,1512140333u,256642560u}},
            {213,new uint[]{3562393206u,4114132955u,2682942662u,1363751946u}},
            {241,new uint[]{147545013u,1217482627u,4075552797u,2561865402u}}, //mystery type
            {243,new uint[]{2488241878u,1253603596u,16950118u,1996268149u}}, //mystery type
            {245,new uint[]{1487190088u,3585884614u,388308249u,2537571603u}}, //mystery type
            {329,new uint[]{714875047u,3848040304u,97826517u,3600056784u}},
            {221,new uint[]{497906049u,3117965308u,1305777954u,3004653388u}},
            {62,new uint[]{1670076494u,877024562u,4127686378u,2181369704u}},
            {49,new uint[]{3785648968u,3932863837u,1688256906u,3364444208u}},
        };
    }
}