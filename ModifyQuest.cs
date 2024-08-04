﻿
namespace MHFQuestReader
{
	public static class ModifyQuest
    {

        public const int cMax_MapID = 0x49;
        public const int cMax_MonsterID = 0x49;
        public const int cMax_ItemID = 0x031D;
        public const int cMax_FishID = 0x0017;

        public const int cMax_GuTi = 0x16;
        public const int cMax_QuestStar = 8;

        public const int cModify_QuestID = 0xEA74;

        /// <summary>
        /// 道具ID超出最大限制时，修改为【不可燃烧的废物】
        /// </summary>
        public const int cModify_OutOfItemID = 0x00AE;

        /// <summary>
        /// 鱼ID超出最大限制时，修改为【刺身鱼】
        /// </summary>
        public const int cModify_OutOfFishID = 0x0002;

        /// <summary>
        /// Dos中无意义数据
        /// </summary>
        public const int cNon0x00For2DosPtr = 19;
        /// <summary>
        /// MHF任务信息偏移
        /// </summary>
        public const int cQuestMHFOffset = 12;
        /// <summary>
        /// 2Dos任务信息偏移
        /// </summary>
        public const int cQuest2DosOffset = 8;
        /// <summary>
        /// 任务信息需偏移长度
        /// </summary>
        public const int cQuestMhfToDosSetLenght = 64;

        /// <summary>
        /// 任务信息 指针组 总长度
        /// </summary>
        public const int cQuest2DosInfoPtrGourpLenght = 72;
        /// <summary>
        /// 移动信息指针组 到的指定位置
        /// </summary>
        public const int cSetInfoPtrGourpMoveToStarPos = 0x88;

        /// <summary>
        /// 任务内容 指针组 到的指定位置
        /// </summary>
        public const int cQuestContenPtrGourpMoveToStarPos = 0xD0;



        /// <summary>
        /// 移动整个任务文本 到的指定位置
        /// </summary>
        public const int cQuestTextAllMsgMoveToStarPos = 0xF0;
        /// <summary>
        /// 移动整个任务文本 到的指定的截止位置
        /// </summary>
        public const int cQuestTextAllMsgMoveToEndPos = 0x1Ff;



        /// <summary>
        /// 任务_类型 偏移
        /// </summary>
        public const int cQuestInfo_Type_Offset = 0;
        /// <summary>
        /// 任务_类型 长度
        /// </summary>
        public const int cQuestInfo_Type_Lenght = 1;

        /// <summary>
        /// 任务_星级 偏移
        /// </summary>
        public const int cQuestInfo_Star_Offset = 4;
        /// <summary>
        /// 任务_星级 长度
        /// </summary>
        public const int cQuestInfo_Star_Lenght = 2;

        /// <summary>
        /// 任务_类型 偏移
        /// </summary>
        public const int cQuestInfo_TargetMap_Offset = 32;

        /// <summary>
        /// 任务_类型 长度
        /// </summary>
        public const int cQuestInfo_TargetMapID_Lenght = 1;

        /// <summary>
        /// 任务_类型 偏移
        /// </summary>
        public const int cQuestInfo_QuestID_Offset = 42 + 4;//MHF还要+4
        /// <summary>
        /// 任务_类型 长度
        /// </summary>
        public const int cQuestInfo_QuestID_Lenght = 2;

        public static bool ReadQuset(byte[] src, out string _QuestName, out List<string> Infos, out uint _QuestID)
		{
            Infos = new List<string>();
            _QuestName = "";
            byte[] target = HexHelper.CopyByteArr(src);//加载数据


            //任务信息
            if (ModifyQuestMap(target, out List<string> out_ModifyQuestMap, out string QuestName, out _QuestID))
            {
                Infos.AddRange(out_ModifyQuestMap);
                if (QuestName == "\u0011")
                {
                    _QuestName = "_";
                }
                else
                {
                    _QuestName = QuestName;
                    _QuestName = _QuestName.Trim(' ');
                    _QuestName = _QuestName.Replace(" ", "");
                    _QuestName = _QuestName.Replace("\r", "");
                    _QuestName = _QuestName.Replace("\n", "");
                    _QuestName = _QuestName.Replace("?", "");
                    _QuestName = _QuestName.Replace("\\", "");
                    _QuestName = _QuestName.Replace("/", "");
                    _QuestName = _QuestName.Replace("=", "");
                }
            }

            //支援道具
            if (FixSuppliesItem(target, out List<string> out_FixSuppliesItem))
            {
                Infos.AddRange(out_FixSuppliesItem);
            }

            //任务报酬
            if (ModifyQuestRewardItem(target, out List<string> out_ModifyQuestRewardItem))
            {
                Infos.AddRange(out_ModifyQuestRewardItem);
            }

            //采集点
            if (FixItemPoint(target, out List<string> out_FixItemPoint))
            {
                Infos.AddRange(out_FixItemPoint);
            }

            //鱼
            if (FixFishGroupPoint(target, out List<string> out_FixFishGroupPoint))
            {
                Infos.AddRange(out_FixFishGroupPoint);
            }

            //if (ModifyTextOffset(target, out byte[] out_ModifyTextOffset))
            //    target = out_ModifyTextOffset;
            //else { return false; }


            //if (ModifyQuestBOSS(target, out byte[] out_ModifyQuestBOSS))
            //    target = out_ModifyQuestBOSS;
            //else { return false; }

            //if (FixMapAreaData(target, out byte[] out_FixMapAreaData))
            //    target = out_FixMapAreaData;
            //else { return false; }

            //else { return false; }




            return true;
        }


        public static bool ModifyQuestMap(byte[] src, out List<string> resultStr,out string QuestName,out uint _QuestID)
        {
            resultStr = new List<string>();
            QuestName = "";
            resultStr.Add("");
            resultStr.Add("【任务基本信息】");
            resultStr.Add("");
            try
            {
                byte[] target = HexHelper.CopyByteArr(src);//加载数据

                //从前4字节取出指针 定位任务信息位置
                int _QuestInfoPtr = HexHelper.bytesToInt(target, 4, 0x00);
                Log.HexTips(0x00, "开始读取任务头部信息,指针->{0}", _QuestInfoPtr);


                //----Step---- 读取任务数据
                //任务类型
                int _QuestType = HexHelper.bytesToInt(target, cQuestInfo_Type_Lenght, _QuestInfoPtr + cQuestInfo_Type_Offset);
                Log.HexInfo(_QuestInfoPtr + cQuestInfo_Type_Offset, "任务类型->{0}", _QuestType);


                //任务星 尝试处理方案
                int _QuestStart = HexHelper.bytesToInt(target, 1, _QuestInfoPtr + cQuestInfo_Star_Offset);
                //if (_QuestStart > cMax_QuestStar)
                //{
                //    Log.HexWar(_QuestInfoPtr + cQuestInfo_Star_Offset, "任务星级超出限制 ->{0},修正为2Dos星最大值{1}", _QuestStart, cMax_QuestStar);
                //}
                //else
                {
                    Log.HexColor(ConsoleColor.Magenta, _QuestInfoPtr + cQuestInfo_Star_Offset, "任务星级->{0}", _QuestStart);
                }
                //Log.HexTips(_QuestInfoPtr + cQuestInfo_Star_Offset, "写入任务星级,MHF为2位,2Dos为1位{0}，覆盖第二位无意义数据", _QuestStart);
                //HexHelper.ModifyIntHexToBytes(target, cMax_QuestStar, _QuestInfoPtr + cQuestInfo_Star_Offset, cQuestInfo_Star_Lenght);


                int _QuestTargetMapID = HexHelper.bytesToInt(target, cQuestInfo_TargetMapID_Lenght, _QuestInfoPtr + cQuestInfo_TargetMap_Offset);
                //if (_QuestTargetMapID > cMax_MapID)
                //{
                //    Log.HexWar(_QuestInfoPtr + cQuestInfo_TargetMap_Offset, "目的地地图,指针->{0} 超过最大 属于MHF地图", _QuestTargetMapID);
                //}
                //else
                {
                    Log.HexColor(ConsoleColor.Green, _QuestInfoPtr + cQuestInfo_TargetMap_Offset, "目的地地图,指针->{0} 【"+MHHelper.Get2MapName(_QuestTargetMapID)+ "】", _QuestTargetMapID);
                }

                int _ModeType = HexHelper.bytesToInt(target, 1, _QuestInfoPtr + 2);
                ////非训练任务
                //if (!MHHelper.CheckIsXunLianMode(_ModeType))
                //{
                //    Log.HexTips(_QuestInfoPtr + 2, "任务模式->原始数据{0}", _ModeType);
                //    //如果是昼地图 但不是昼模式
                //    if (MHHelper.CheckIsDayMapID(_QuestTargetMapID)
                //        &&
                //        !MHHelper.CheckIsDayMode(_ModeType)
                //        )
                //    {
                //        HexHelper.ModifyIntHexToBytes(target, 0x1C, _QuestInfoPtr + 2, 1);
                //        Log.HexWar(_QuestInfoPtr + 2, "任务模式->修改白天 为{0}", 0x1C);
                //    }
                //    //如果是夜地图 但不是夜模式
                //    else if (MHHelper.CheckIsNightMapID(_QuestTargetMapID)
                //        &&
                //        !MHHelper.CheckIsNightMode(_ModeType)
                //        )
                //    {
                //        HexHelper.ModifyIntHexToBytes(target, 0x12, _QuestInfoPtr + 2, 1);
                //        Log.HexWar(_QuestInfoPtr + 2, "任务模式->修改黑夜 为{0}", 0x12);
                //    }
                //}
                //else
                //{
                //    Log.HexTips(_QuestInfoPtr + 2, "任务模式 原始数据 是训练模式 ->{0}", _ModeType);
                //}


                _QuestID = HexHelper.bytesToUInt(target, cQuestInfo_QuestID_Lenght, _QuestInfoPtr + cQuestInfo_QuestID_Offset);
                Log.HexTips(_QuestInfoPtr + cQuestInfo_QuestID_Offset, "任务编号【{0}】", _QuestID);
				//if (_QuestID < 60000)
				//{
				//    HexHelper.ModifyIntHexToBytes(target, cModify_QuestID, _QuestInfoPtr + cQuestInfo_QuestID_Offset, cQuestInfo_QuestID_Lenght);
				//    Log.HexTips(_QuestInfoPtr + cQuestInfo_QuestID_Offset, "任务编号【{0}】小于60000，修正为【{1}】,使其可下载", _QuestID, cModify_QuestID);
				//}

				//从前4字节取出指针 定位任务信息位置
				int _QuestContentPtr = HexHelper.bytesToInt(target, 4, _QuestInfoPtr + 36 + 4);//MHF还要+4
                Log.HexTips(_QuestInfoPtr + 24, "读取任务内容指针,指针->{0}", _QuestContentPtr);

                int _QuestNametPtr = HexHelper.bytesToInt(target, 4, _QuestContentPtr);
                QuestName = HexHelper.ReadBytesToString(src, _QuestNametPtr);



                Log.HexColor(ConsoleColor.Green,_QuestNametPtr, "任务名称:" + QuestName); ;


                //固体值
                int _GuTiValue = HexHelper.bytesToInt(target, 4, 0x48);


                resultStr.Add("[任务名称]");
                resultStr.Add(QuestName);

                resultStr.Add("[任务编号]");
                resultStr.Add(_QuestID.ToString());

                resultStr.Add("[任务星级]");
                resultStr.Add(_QuestStart.ToString());

                resultStr.Add("[任务模式值]");
                resultStr.Add(_ModeType.ToString());

                resultStr.Add("[任务地图]");
                resultStr.Add(MHHelper.Get2MapName(_QuestTargetMapID));

                resultStr.Add("[固体值(怪物强度)]");
                resultStr.Add(_GuTiValue.ToString());

                //if (_GuTiValue > cMax_GuTi)
                //{
                //    Log.HexWar(0x48, "固体值超出限制 ->{0},修正为2Dos最大值{1}", _GuTiValue, cMax_GuTi);
                //    HexHelper.ModifyIntHexToBytes(target, cMax_GuTi, 0x48, 4);
                //}
                //else
                {
                    Log.HexColor(ConsoleColor.Blue, 0x48, "固体值 ->{0}", _GuTiValue);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //target = null;
                _QuestID = 0;
                return false;
            }
        }

        /// <summary>
        /// 轮询单个报酬组的数据
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <param name="_RewardGroupPtr"></param>
        /// <returns></returns>
        static bool QuestRewardGroup(byte[] src,int _RewardGroupPtr, out List<string> resultStr)
        {
            resultStr = new List<string>();
            //加载数据
            byte[] target = HexHelper.CopyByteArr(src);
            //读取报酬游标
            int CurrPtr = _RewardGroupPtr;
            bool isFinish = false;
            int setCount = 0;
            while (!isFinish)
            {
                //若遇到结束符
                if (MHHelper.CheckEnd(target, CurrPtr))
                {
                    isFinish = true;
                    Log.HexInfo(CurrPtr, "遇报酬组结束符");
                }
                else
                {
                    setCount++;
                    int Pr = HexHelper.bytesToInt(target, 2, CurrPtr);//概率
                    int ItemID = HexHelper.bytesToInt(target, 2, CurrPtr + 0x02);//道具ID
                    int count = HexHelper.bytesToInt(target, 2, CurrPtr + 0x04);//数量

                    if (count > 0)
                    {
                        resultStr.Add($"{MHHelper.Get2DosItemName(ItemID)}|概率:{Pr}|数量:{count}");
                    }

                    CurrPtr += 0x06;//前推游标
                }
            }
            return true;
        }

        public static bool ModifyQuestBOSS(byte[] src, out byte[] target)
        {
            try
            {
                target = HexHelper.CopyByteArr(src);//加载数据
                //BOSS(头部信息指针
                int _BOOSInFoPtr = HexHelper.bytesToInt(target, 4, 0x18);

                Log.HexTips(0x18, "开始读取BOSS(头部信息,指针->{0}", _BOOSInFoPtr);

                //BOSS组指针
                int _BOOSStarPtr = HexHelper.bytesToInt(target, 4, _BOOSInFoPtr + 0x08);

                Log.HexTips(_BOOSInFoPtr + 0x08, "第一个BOSS指针->{0}", _BOOSStarPtr);

                //读取BOSS组游标
                int CurrPtr = _BOOSStarPtr;
                bool isFinish = false;

                int BOSSIndex = 0;
                //循环取BOSS组
                while (!isFinish)
                {
                    //若遇到结束符或无数据
                    if (MHHelper.CheckEnd(target, CurrPtr)
                        ||
                        HexHelper.bytesToInt(target,1, CurrPtr) == 0
                        )
                    {
                        isFinish = true;
                        Log.HexInfo(CurrPtr, "遇BOSS组信息结束符或无数据");
                    }
                    else
                    {
                        BOSSIndex++;
                        //报酬组类型
                        int _BOSSID = HexHelper.bytesToInt(target, 0x04, CurrPtr);

                        if (_BOSSID > cMax_MonsterID)
                        {
                            Log.HexWar(CurrPtr, "第{0}个BOSS，ID->{1} 大于了 最大ID{2} 属于MHF怪物,该任务无法使用", BOSSIndex, _BOSSID, cMax_MonsterID);
                        }
                        else
                        {
                            Log.HexColor(ConsoleColor.Green, CurrPtr, "第{0}个BOSS，ID->{1} 【" + MHHelper.Get2BossName(_BOSSID) + "】", BOSSIndex, _BOSSID);
                        }

                        CurrPtr += 0x04;//前推游标
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); target = null; return false;
            }
        }

        public static bool FixMapAreaData(byte[] src,out byte[] target)
        {
            int _QuestTargetMapID;
            try
            {
                target = HexHelper.CopyByteArr(src);//加载数据

                //从前4字节取出指针 定位任务信息位置
                int _QuestInfoPtr = HexHelper.bytesToInt(target, 4, 0x00);
                Log.HexTips(0x00, "开始读取任务头部信息,指针->{0}", _QuestInfoPtr);

                //任务目的地MapID
                _QuestTargetMapID = HexHelper.bytesToInt(target, ModifyQuest.cQuestInfo_TargetMapID_Lenght, _QuestInfoPtr + ModifyQuest.cQuestInfo_TargetMap_Offset);
                Log.HexColor(ConsoleColor.Green, _QuestInfoPtr + ModifyQuest.cQuestInfo_TargetMap_Offset, "目的地地图,指针->{0} 【" + MHHelper.Get2MapName(_QuestTargetMapID) + "】", _QuestTargetMapID);



                if (LoadToSaveTemplate.DictMapAreaData.ContainsKey(_QuestTargetMapID))
                {
                    //区域数量
                    int _AreaCount = MHHelper.GetMapAreaCount(_QuestTargetMapID);
                    Log.Info(MHHelper.Get2MapName(_QuestTargetMapID) + "的地图数量" + _AreaCount);

                    MapAreaData srcData2Dos = LoadToSaveTemplate.DictMapAreaData[_QuestTargetMapID];
                    #region 换区设置

                    //换区设置指针
                    int _CAreaSetTopPtr = HexHelper.bytesToInt(target, 4, 0x1C);
                    Log.HexInfo(0x1C, "换区设置指针->{0}", _CAreaSetTopPtr);

                    //读取换区单个区域游标
                    int _CAreaSetTop_CurrPtr = _CAreaSetTopPtr;

                    for (int i = 0; i < _AreaCount; i++)
                    {
                        int _One_CurrPtr = HexHelper.bytesToInt(target, 4, _CAreaSetTop_CurrPtr);

                        if (_One_CurrPtr == 0x0)
                        {
                            Log.HexInfo(_CAreaSetTop_CurrPtr, "区域设置" + i + "指针为0，跳过");
                            break;
                        }

                        if (srcData2Dos.targetDatas.Length <= i)
                        {
                            Log.HexWar(_One_CurrPtr, "第" + i + "区 区域设置,比2Dos区数超限。");
                            break;
                        }


                        int Set_TargetIndex = 0;
                        while (true)
                        {
                            if (MHHelper.CheckEnd(target, _One_CurrPtr)
                            ||
                            HexHelper.bytesToInt(target, 1, _One_CurrPtr) == 0)
                            {
                                Log.HexInfo(_One_CurrPtr, "区域设置结束符");
                                break;
                            }

                            if (srcData2Dos.targetDatas[i].targetData.Count <= Set_TargetIndex)
                            {
                                Log.HexWar(_One_CurrPtr, "第" + i + "区,第" + Set_TargetIndex + "个目标,比2Dos目标数超限。");
                                break;
                            }

                            byte[] srcOneData = srcData2Dos.targetDatas[i].targetData[Set_TargetIndex];

                            HexHelper.ModifyDataToBytes(target, srcOneData, _One_CurrPtr);
                            Log.HexTips(_One_CurrPtr, "第" + i + "区，第" + Set_TargetIndex + "个目标，更换为2Dos数据，长度{0}", srcOneData.Length);

                            Set_TargetIndex++;
                            _One_CurrPtr += 0x34;
                        }

                        _CAreaSetTop_CurrPtr += 0x4;
                    }
                    #endregion

                    #region 区域映射
                    //区域映射指针
                    int _CAreaPosTopPtr = HexHelper.bytesToInt(target, 4, 0x20);
                    Log.HexInfo(0x20, "换区映射指针->{0}", _CAreaPosTopPtr);
                    //读取单个区域映射游标
                    int _CAreaPosTop_CurrPtr = _CAreaPosTopPtr;
                    for (int i = 0; i < _AreaCount; i++)
                    {
                        if (srcData2Dos.targetDatas.Length <= i)
                        {
                            Log.HexWar(_CAreaPosTop_CurrPtr, "第" + i + "区 换区映射,比2Dos区数超限。");
                            break;
                        }
                        byte[] srcOneData = srcData2Dos.areaPosDatas[i];

                        HexHelper.ModifyDataToBytes(target, srcOneData, _CAreaPosTop_CurrPtr);
                        Log.HexTips(_CAreaPosTop_CurrPtr, "第" + i + "区的区域映射，更换为2Dos数据，读取数据,长度{0}", srcOneData.Length);
                        _CAreaPosTop_CurrPtr += 0x20;
                    }
                    #endregion

                }
                else
                {
                    Log.HexColor(ConsoleColor.Green, _QuestInfoPtr + ModifyQuest.cQuestInfo_TargetMap_Offset, "目的地地图,在源数据之外");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                target = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 报酬
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool ModifyQuestRewardItem(byte[] src,out List<string> resultStr)
        {
            resultStr = new List<string>();

            resultStr.Add("");
            resultStr.Add("【任务报酬】");
            resultStr.Add("");

            try
            {

                byte[] target = HexHelper.CopyByteArr(src);//加载数据
                //任务报酬信息指针
                int _QuestRewardPtr = HexHelper.bytesToInt(target, 4, 0x0C);

                Log.HexTips(0x0C, "开始读取报酬组头部信息,指针->{0}", _QuestRewardPtr); ;

                //读取组报酬游标
                int CurrPtr = _QuestRewardPtr;
                bool isFinish = false;

                int GroupIndex = 0;
                //循环取道具组
                while (!isFinish)
                {
                    //若遇到结束符
                    if (MHHelper.CheckEnd(target, CurrPtr))
                    {
                        isFinish = true;
                        Log.HexInfo(CurrPtr, "遇报酬组头部信息结束符");
                    }
                    else
                    {
                        GroupIndex++;
                        //报酬组类型
                        int _RewardCondition = HexHelper.bytesToInt(target, 0x04, CurrPtr);
                        //报酬组指针
                        int _RewardGroupPtr = HexHelper.bytesToInt(target, 0x04, CurrPtr + 0x04);

                        Log.HexTips(CurrPtr, "第{0}报酬组，报酬类型->{1} 报酬组指针->{2}", GroupIndex, _RewardCondition, _RewardGroupPtr);

                        resultStr.Add($"--[第{GroupIndex}组报酬]");

                        //取组内报酬
                        if (QuestRewardGroup(target, _RewardGroupPtr, out List<string> Single_resultStr))
                        {
                            resultStr.AddRange(Single_resultStr);
                        }
                        CurrPtr += 0x08;//前推游标 读取下一个报酬道具组
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); 
                return false;
            }
        }

        public static bool FixSuppliesItem(byte[] src, out List<string> resultStr)
        {
            resultStr = new List<string>();
            resultStr.Add("");
            resultStr.Add("【支援道具报酬】");
            resultStr.Add("");
            try
            {
                byte[] target = HexHelper.CopyByteArr(src);//加载数据
                //支援道具指针
                int _SuppliesItemPtr = HexHelper.bytesToInt(target, 4, 0x08);
                Log.HexTips(0x08, "开始读取支援道具指针,指针->{0}", _SuppliesItemPtr);

                int _SuppliesItem_CurrPtr = _SuppliesItemPtr;



                resultStr.Add("--[主线支援道具]");

                for (int i = 0; i < 96; i++)
                {
                    //若遇到结束符或无数据
                    if (MHHelper.CheckEnd(target, _SuppliesItem_CurrPtr)
                        ||
                        HexHelper.bytesToInt(target, 4, _SuppliesItem_CurrPtr) == 0
                        )
                    {
                        Log.HexInfo(_SuppliesItem_CurrPtr, "主线支援道具，结束符");
                        break;
                    }

                    int ItemID = HexHelper.bytesToInt(target, 2, _SuppliesItem_CurrPtr);//道具ID
                    int Count = HexHelper.bytesToInt(target, 2, _SuppliesItem_CurrPtr + 0x02);//数量

                    resultStr.Add($"{MHHelper.Get2DosItemName(ItemID)}  |   数量{Count}");

                    ////判断道具ID是否超限
                    //if (ItemID > cMax_ItemID)
                    //{
                    //    Log.HexWar(_SuppliesItem_CurrPtr, "主线支援道具,第" + i + "个ID->{0}道具ID超出最大可能{1}，属于MHF道具【" + MHHelper.Get2MHFItemName(ItemID) + "】,将其修正为【不可燃烧的废物】ID->{2}", ItemID, cMax_ItemID, cModify_OutOfItemID);
                    //    HexHelper.ModifyIntHexToBytes(target, cModify_OutOfItemID, _SuppliesItem_CurrPtr, 2);
                    //}
                    //else
                    //{
                    //    Log.HexColor(ConsoleColor.Green, _SuppliesItem_CurrPtr, "主线支援道具第" + i + "个，道具ID->{0} 【" + MHHelper.Get2DosItemName(ItemID) + "】 数量->{1}", ItemID, Count);
                    //}

                    _SuppliesItem_CurrPtr += 0x04;
                }


                resultStr.Add("--[支线1支援道具]");

                int _SuppliesItem_Zhi_1_CurrPtr = _SuppliesItemPtr + 0x60;
                for (int i = 0; i < 32; i++)
                {
                    //若遇到结束符或无数据
                    if (MHHelper.CheckEnd(target, _SuppliesItem_Zhi_1_CurrPtr)
                        ||
                        HexHelper.bytesToInt(target, 4, _SuppliesItem_Zhi_1_CurrPtr) == 0
                        )
                    {
                        Log.HexInfo(_SuppliesItem_Zhi_1_CurrPtr, "支线1支援道具，结束符");
                        break;
                    }

                    int ItemID = HexHelper.bytesToInt(target, 2, _SuppliesItem_Zhi_1_CurrPtr);//道具ID
                    int Count = HexHelper.bytesToInt(target, 2, _SuppliesItem_Zhi_1_CurrPtr + 0x02);//数量

                    resultStr.Add($"{MHHelper.Get2DosItemName(ItemID)}  |   数量{Count}");
                    ////判断道具ID是否超限
                    //if (ItemID > cMax_ItemID)
                    //{
                    //    Log.HexWar(_SuppliesItem_Zhi_1_CurrPtr, "支线1支援道具第" + i + "个,ID->{0}道具ID超出最大可能{1}，属于MHF道具【" + MHHelper.Get2MHFItemName(ItemID) + "】,将其修正为【不可燃烧的废物】ID->{2}", ItemID, cMax_ItemID, cModify_OutOfItemID);
                    //    HexHelper.ModifyIntHexToBytes(target, cModify_OutOfItemID, _SuppliesItem_Zhi_1_CurrPtr, 2);
                    //}
                    //else
                    //{
                    //    Log.HexColor(ConsoleColor.Green, _SuppliesItem_Zhi_1_CurrPtr, "支线1支援道具第" + i + "个主线，道具ID->{0} 【" + MHHelper.Get2DosItemName(ItemID) + "】 数量->{1}", ItemID, Count);
                    //}

                    _SuppliesItem_Zhi_1_CurrPtr += 0x04;
                }



                resultStr.Add("--[支线2支援道具]");
                int _SuppliesItem_Zhi_2_CurrPtr = _SuppliesItemPtr + 0x60 + 0x20;
                for (int i = 0; i < 32; i++)
                {
                    //若遇到结束符或无数据
                    if (MHHelper.CheckEnd(target, _SuppliesItem_Zhi_2_CurrPtr)
                        ||
                        HexHelper.bytesToInt(target, 4, _SuppliesItem_Zhi_2_CurrPtr) == 0
                        )
                    {
                        Log.HexInfo(_SuppliesItem_Zhi_2_CurrPtr, "支线2支援道具，结束符");
                        break;
                    }

                    int ItemID = HexHelper.bytesToInt(target, 2, _SuppliesItem_Zhi_2_CurrPtr);//道具ID
                    int Count = HexHelper.bytesToInt(target, 2, _SuppliesItem_Zhi_2_CurrPtr + 0x02);//数量

                    resultStr.Add($"{MHHelper.Get2DosItemName(ItemID)}  |   数量{Count}");
                    ////判断道具ID是否超限
                    //if (ItemID > cMax_ItemID)
                    //{
                    //    Log.HexWar(_SuppliesItem_Zhi_2_CurrPtr, "支线2支援道具第" + i + "个,ID->{0}道具ID超出最大可能{1}，属于MHF道具【" + MHHelper.Get2MHFItemName(ItemID) + "】,将其修正为【不可燃烧的废物】ID->{2}", ItemID, cMax_ItemID, cModify_OutOfItemID);
                    //    HexHelper.ModifyIntHexToBytes(target, cModify_OutOfItemID, _SuppliesItem_Zhi_2_CurrPtr, 2);
                    //}
                    //else
                    //{
                    //    Log.HexColor(ConsoleColor.Green, _SuppliesItem_Zhi_2_CurrPtr, "支线2支援道具第" + i + "个主线，道具ID->{0} 【" + MHHelper.Get2DosItemName(ItemID) + "】 数量->{1}", ItemID, Count);
                    //}

                    _SuppliesItem_Zhi_2_CurrPtr += 0x04;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //target = null;
                return false;
            }
            return true;
        }

        public static bool FixItemPoint(byte[] src,out List<string> resultStr)
        {
            resultStr = new List<string>();
            resultStr.Add("");
            resultStr.Add("【采集点信息】");
            resultStr.Add("");
            try
            {
                byte[] target = HexHelper.CopyByteArr(src);//加载数据
                //采集点指针
                int _ItemPointPtr = HexHelper.bytesToInt(target, 4, 0x38);
                Log.HexTips(0x38, "开始读取采集点指针,指针->{0}", _ItemPointPtr);

                int _ItemPoint_CurrPtr = _ItemPointPtr;

                for (int i = 0; i < 90; i++)
                {
                    //若遇到结束符或无数据
                    if (MHHelper.CheckEnd(target, _ItemPoint_CurrPtr)
                        //||
                        //HexHelper.bytesToInt(target, 1, _ItemPoint_CurrPtr) == 0 // 不能判断头部为0 否则当前道具概率为0时，会跳过
                        )
                    {
                        Log.HexInfo(_ItemPoint_CurrPtr, "采集点结束");
                        break;
                    }

                    resultStr.Add($"--[采集代号{i}]");

                    if (i == 59)
                    {
                        
                    }
                    int ItemStartPtr = HexHelper.bytesToInt(target, 4, _ItemPoint_CurrPtr);


                    int ItemCurrPtr = ItemStartPtr;

                    int setCount = 0;
                    while (true)
                    {
                        //若遇到结束符或无数据
                        if (MHHelper.CheckEnd(target, ItemCurrPtr)
                            //||
                            //HexHelper.bytesToInt(target, 1, ItemCurrPtr) == 0 // 不能判断值为0 否则当前道具概率为0时，会跳过
                            )
                        {
                            Log.HexInfo(ItemCurrPtr, "第{0}个采集代号，第" + setCount + "个素材 结束符",i);
                            break;
                        }
                        int Pr = HexHelper.bytesToInt(target, 2, ItemCurrPtr);//概率
                        int ItemID = HexHelper.bytesToInt(target, 2, ItemCurrPtr + 0x02);//道具ID

                        if (Pr > 0 && ItemID > 0 && ItemID <= 0x031D)
                        {
                            resultStr.Add($"道具：{MHHelper.Get2DosItemName(ItemID)}，概率：{Pr}");
                        }

                        ////判断道具ID是否超限
                        //if (ItemID > cMax_ItemID)
                        //{
                        //    Log.HexWar(ItemCurrPtr, "第{0}个采集代号，第" + setCount + "个素材，ID->{1}道具ID超出最大可能{2}，属于MHF道具【" + MHHelper.Get2MHFItemName(ItemID) + "】,将其修正为【不可燃烧的废物】ID->{3}", i,ItemID, cMax_ItemID, cModify_OutOfItemID);
                        //    HexHelper.ModifyIntHexToBytes(target, cModify_OutOfItemID, ItemCurrPtr + 0x02, 2);
                        //    ItemID = HexHelper.bytesToInt(target, 2, ItemCurrPtr + 0x02);//道具ID
                        //    Log.HexTips(ItemCurrPtr, "重新读取 第{0}个采集代号，第" + setCount + "个素材，道具ID->{1} 【" + MHHelper.Get2DosItemName(ItemID) + "】 概率->{2}", i,ItemID, Pr);
                        //}
                        //else
                        //{
                        //    Log.HexColor(ConsoleColor.Green, ItemCurrPtr, "第{0}个采集代号，第" + setCount + "个素材，道具ID->{1} 【" + MHHelper.Get2DosItemName(ItemID) + "】 概率->{2}", i,ItemID, Pr);
                        //}
                        setCount++;
                        ItemCurrPtr += 0x04;
                    }
                    _ItemPoint_CurrPtr += 0x04;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //target = null;
                return false;
            }
            return true;
        }

        public static bool FixFishGroupPoint(byte[] src,out List<string> resultStr)
        {
            resultStr = new List<string>();
            resultStr.Add("");
            resultStr.Add("【钓鱼点信息】");
            resultStr.Add("");

            try
            {
                byte[] target = HexHelper.CopyByteArr(src);//加载数据
                //鱼群指针
                int _FishGroupPtr = HexHelper.bytesToInt(target, 4, 0x40);
                Log.HexTips(0x40, "开始读取鱼群信息,指针->{0}", _FishGroupPtr);
                int _FishGroup_CurrPtr = _FishGroupPtr;

                int setFishGroup = 0;
                //鱼群代号 循环
                while (true)
                {
                    //鱼群代号结束符
                    if (
                        _FishGroup_CurrPtr >= target.Length
                        ||
                        MHHelper.CheckEnd(target, _FishGroup_CurrPtr)
                        ||
                        HexHelper.bytesToInt(target, 4, _FishGroup_CurrPtr) == 0
                        )
                    {
                        Log.HexInfo(_FishGroup_CurrPtr, $"第{setFishGroup}鱼群代号 结束符");
                        break;
                    }



                    //鱼群季节循环
                    int _FishSeasonStartPtr = HexHelper.bytesToInt(target, 4, _FishGroup_CurrPtr);
                    int _FishSeason_CurrPtr = _FishSeasonStartPtr;


                    //鱼群季节循环
                    for (int i = 0; i < 6; i++)
                    {
                        //鱼群季节结束符
                        if (
                            _FishSeason_CurrPtr >= target.Length
                            ||
                            MHHelper.CheckEnd(target, _FishSeason_CurrPtr)
                            ||
                            HexHelper.bytesToInt(target, 1, _FishSeason_CurrPtr) == 0
                            )
                        {
                            Log.HexInfo(_FishSeason_CurrPtr, $"第{setFishGroup}鱼群代号 第{i}个季节昼夜 结束符");
                            break;
                        }

                        string DayType = "";
                        switch(i)
                        {
                            case 0: DayType = "温暖期|白天"; break;
                            case 1: DayType = "繁殖期|白天"; break;
                            case 2: DayType = "寒冷期|白天"; break;
                            case 3: DayType = "温暖期|夜晚"; break;
                            case 4: DayType = "繁殖期|夜晚"; break;
                            case 5: DayType = "寒冷期|夜晚"; break;
                        }

                        resultStr.Add($"--[鱼群代号{setFishGroup} - {DayType}]");

                        int _FishStartPtr = HexHelper.bytesToInt(target, 4, _FishSeason_CurrPtr);
                        int _FishStart_CurrPtr = _FishStartPtr;

                        int setFish = 0;
                        while (true)
                        {
                            //鱼结束符
                            if (
                                _FishStart_CurrPtr >= target.Length
                                ||
                                MHHelper.CheckEndWith1Byte(target, _FishStart_CurrPtr)
                                //||
                                //HexHelper.bytesToInt(target, 1, _FishStart_CurrPtr) == 0
                                )
                            {
                                Log.HexInfo(_FishStart_CurrPtr, $"第{setFishGroup}鱼群代号 第{i}个季节昼夜 第" + setFish + "个鱼 结束符");
                                break;
                            }

                            int Pr = HexHelper.bytesToInt(target, 1, _FishStart_CurrPtr);//概率
                            int FishID = HexHelper.bytesToInt(target, 1, _FishStart_CurrPtr + 0x01);//鱼ID



                            if (Pr > 0 && FishID > 0)
                            {
                                resultStr.Add($"{MHHelper.Get2DosFishName(FishID)} |   概率：{Pr}");
                            }

                            ////判断道具ID是否超限
                            //if (FishID > cMax_FishID)
                            //{
                            //    Log.HexWar(_FishStart_CurrPtr, "第" + setFishGroup + "鱼群，第" + i + "个季节昼夜，第" + setFish + "个鱼 鱼ID->{0} 超出2Dos最大值{1}，修正为【刺身鱼】{2}", FishID, cMax_FishID, cModify_OutOfFishID);
                            //    HexHelper.ModifyIntHexToBytes(target, cModify_OutOfFishID, _FishStart_CurrPtr + 0x01, 1);
                            //}
                            //else
                            //{
                            //    Log.HexColor(ConsoleColor.Green, _FishStart_CurrPtr, "第" + setFishGroup + "鱼群，第" + i + "个季节昼夜，第" + setFish + "个鱼 鱼ID->{0}【"+MHHelper.Get2DosFishName(FishID)+"】 概率->{1}", FishID, Pr);
                            //}

                            setFish++;
                            _FishStart_CurrPtr += 0x02;
                        }

                        _FishSeason_CurrPtr += 0x08;
                    }

                    setFishGroup++;
                    _FishGroup_CurrPtr += 0x04;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }
    }
}
