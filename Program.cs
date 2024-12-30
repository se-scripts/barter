using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        /*
         * R e a d m e
         * -----------
         * 太空工程师 以物易物脚本。
         * 
         * @see <https://github.com/se-scripts/barter>
         * @author [chivehao](https://github.com/chivehao)
         */
        const string version = "1.0.0";
        MyIni _ini = new MyIni();

        const string translateListSection = "TranslateList", informationSection = "Information", lengthKey = "Length";
        const string barterConfigSection = "BarterConfig";

        List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
        List<IMyTextPanel> panels = new List<IMyTextPanel>();
        List<IMyTextPanel> panels_Items_All = new List<IMyTextPanel>();
        List<IMyTextPanel> panels_Overall = new List<IMyTextPanel>();
        Dictionary<string, string> translator = new Dictionary<string, string>();
        double counter_Logo = 0;


        Color background_Color = new Color(0, 35, 45);
        Color border_Color = new Color(0, 130, 255);


        public struct BarterRelation
        {
            public string source;
            public string target;
            public double Ratio;
        }

        public struct Goods {
            public BarterRelation BarterRelation;
            public bool IsSource;
            public string Name;
            public long Amount;
        }

        /// <summary>
        /// 交易箱 PublicItemCargo
        /// </summary>
        List<IMyCargoContainer> tradeCargos = new List<IMyCargoContainer>();
        /// <summary>
        /// 存货箱 PrivateItemCargo
        /// </summary>
        List<IMyCargoContainer> stockCargos = new List<IMyCargoContainer>();
        /// <summary>
        /// 商品列表LCD  LCD_ITEM_LIST
        /// </summary>
        IMyTextPanel goodsListLcd;
        /// <summary>
        /// 兑换关系列表
        /// </summary>
        List<BarterRelation> barterRelations = new List<BarterRelation>();
        /// <summary>
        /// 商品列表
        /// </summary>
        List<Goods> goodsList = new List<Goods>();
        /// <summary>
        /// 当前页的商品列表
        /// </summary>
        List<Goods> currentPageGoods = new List<Goods>();
        static Goods emptyGoods = new Goods();
        private Goods selectGoods = emptyGoods;
        int page = 1, size = 15;



        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Once;

            SetDefultConfiguration();

            BuildTranslateDic();

            ProgrammableBlockScreen();

            GridTerminalSystem.GetBlocksOfType(tradeCargos, b => b.IsSameConstructAs(Me) && b.CustomData.Contains("PublicItemCargo"));
            GridTerminalSystem.GetBlocksOfType(stockCargos, b => b.IsSameConstructAs(Me) && b.CustomData.Contains("PrivateItemCargo"));

            goodsListLcd = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("LCD_ITEM_LIST");

            BuildBarterRelations();

            

            ReloadGoodsLcdGoodsList();
        }

        public void SetDefultConfiguration()
        {
            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
                throw new Exception(result.ToString());

            string dataTemp;
            dataTemp = Me.CustomData;
            if (dataTemp == "" || dataTemp == null)
            {
                _ini.Set(informationSection, "LCD_Overall_Display", "LCD_Overall_Display | Fill In CustomName of Panel");
                _ini.Set(informationSection, "LCD_Inventory_Display", "LCD_Inventory_Display:X | X=1,2,3... | Fill In CustomName of Panel");
                _ini.Set(translateListSection, lengthKey, "1");
                _ini.Set(translateListSection, "1", "AH_BoreSight:More");
                _ini.Set(barterConfigSection, lengthKey, "1");
                _ini.Set(barterConfigSection, "1", "Source:Target:10");
                Me.CustomData = _ini.ToString();
            }

            GridTerminalSystem.GetBlocksOfType(cargoContainers, b => b.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType(panels, b => b.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType(panels_Overall, b => b.IsSameConstructAs(Me) && b.CustomName.Contains("LCD_Overall_Display"));
            GridTerminalSystem.GetBlocksOfType(panels_Items_All, b => b.IsSameConstructAs(Me) && b.CustomName.Contains("LCD_Inventory_Display:"));

        }

        public void BuildTranslateDic()
        {
            string value;
            GetConfigurationFromCustomData(Me.CustomData, translateListSection, lengthKey, out value);
            int length = Convert.ToInt16(value);

            for (int i = 1; i <= length; i++)
            {
                GetConfigurationFromCustomData(Me.CustomData, translateListSection, i.ToString(), out value);
                string[] result = value.Split(':');

                translator.Add(result[0], result[1]);
            }
        }


        public void WriteConfigurationToCustomData(out string customData, string section, string key, string value)
        {
            _ini.Set(section, key, value);
            customData = _ini.ToString();
        }


        public void GetConfigurationFromCustomData(string customData, string section, string key, out string value)
        {
            // This time we _must_ check for failure since the user may have written invalid ini.
            MyIniParseResult result;
            if (!_ini.TryParse(customData, out result))
                throw new Exception(result.ToString());

            string DefaultValue = "";

            // Read the integer value. If it does not exist, return the default for this value.
            value = _ini.Get(section, key).ToString(DefaultValue);

        }

        public void ProgrammableBlockScreen()
        {

            //  512 X 320
            IMyTextSurface panel = Me.GetSurface(0);

            if (panel == null) return;
            panel.ContentType = ContentType.SCRIPT;

            MySpriteDrawFrame frame = panel.DrawFrame();

            float x = 512 / 2, y1 = 205;
            DrawLogo(frame, x, y1, 200);
            PanelWriteText(frame, "SE barter scripts\nBy Chivehao With version " + version, x, y1 + 110, 1f, TextAlignment.CENTER);

            frame.Dispose();

        }

        public void DrawLogo(MySpriteDrawFrame frame, float x, float y, float width)
        {
            MySprite sprite = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Screen_LoadingBar",
                Position = new Vector2(x, y),
                Size = new Vector2(width - 6, width - 6),
                RotationOrScale = Convert.ToSingle(counter_Logo / 360 * 2 * Math.PI),
                Alignment = TextAlignment.CENTER,
            };
            frame.Add(sprite);

            sprite = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Screen_LoadingBar",
                Position = new Vector2(x, y),
                Size = new Vector2(width / 2, width / 2),
                RotationOrScale = Convert.ToSingle(2 * Math.PI - counter_Logo / 360 * 2 * Math.PI),
                Alignment = TextAlignment.CENTER,
            };
            frame.Add(sprite);

            sprite = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Screen_LoadingBar",
                Position = new Vector2(x, y),
                Size = new Vector2(width / 4, width / 4),
                RotationOrScale = Convert.ToSingle(Math.PI + counter_Logo / 360 * 2 * Math.PI),
                Alignment = TextAlignment.CENTER,
            };
            frame.Add(sprite);

        }

        public void PanelWriteText(MySpriteDrawFrame frame, string text, float x, float y, float fontSize, TextAlignment alignment)
        {
            MySprite sprite = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = text,
                Position = new Vector2(x, y),
                RotationOrScale = fontSize,
                Color = Color.Coral,
                Alignment = alignment,
                FontId = "LoadingScreen"
            };
            frame.Add(sprite);
        }

        public string TranslateName(string name)
        {
            if (translator.ContainsKey(name))
            {
                return translator[name];
            }
            else
            {
                return ShortName(name);
            }
        }

        public string ShortName(string name)
        {
            string[] temp = name.Split('/');

            if (temp.Length == 2)
            {
                return temp[1];
            }
            else
            {
                return name;
            }
        }

        public void BuildBarterRelations()
        {
            string value;
            GetConfigurationFromCustomData(Me.CustomData, barterConfigSection, lengthKey, out value);
            int length = Convert.ToInt16(value);

            for (int i = 1; i <= length; i++)
            {
                GetConfigurationFromCustomData(Me.CustomData, barterConfigSection, i.ToString(), out value);
                string[] result = value.Split(':');
                BarterRelation barterRelation = new BarterRelation();
                barterRelation.source = result[0];
                barterRelation.target = result[1];
                barterRelation.Ratio = double.Parse(result[2]);
                barterRelations.Add(barterRelation);
            }
        }

        /// <summary>
        /// 从内部箱子取，只要[BarterConfigSection]列表里的第二项的type符合，则构建一个Goods添加到列表。
        /// </summary>
        public void BuildGoodsList()
        {
            goodsList.Clear();
            foreach (var cargo in stockCargos) { 
                if (cargo == null || cargo.InventoryCount == 0) continue;
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                cargo.GetInventory().GetItems(items);
                foreach (var item in items) { 
                    if (item == null || item.Amount <= 0) continue;
                    var type = item.Type.ToString();
                    bool exists = barterRelations.Exists((e) => e.target == type);
                    if (!exists) continue;
                    var brIndex = barterRelations.FindIndex((e) => e.target == type);
                    if (brIndex < 0) continue;
                    var barterRelation = barterRelations[brIndex];
                    var index = goodsList.FindIndex((g) => g.Name.Equals(type));
                    if (index >= 0)
                    {
                        var g = goodsList[index];
                        goodsList.RemoveAt(index);
                        g.Amount = g.Amount + (item.Amount.ToIntSafe()); 
                        g.BarterRelation = barterRelation;
                        g.IsSource = false;
                        g.Name = type;
                        goodsList.Add(g);
                    } else {
                        Goods g = new Goods();
                        g.BarterRelation = barterRelation;
                        g.IsSource = false;
                        g.Name = type;
                        g.Amount = item.Amount.ToIntSafe();
                        goodsList.Add(g);
                    }
                   

                }
            }

        }

        public void DebugLCD(string text)
        {
            List<IMyTextPanel> debugPanel = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(debugPanel, b => b.IsSameConstructAs(Me) && b.CustomName == "DEBUGLCD");

            if (debugPanel.Count == 0) return;

            string temp = "";
            foreach (var panel in debugPanel)
            {
                temp = "";
                temp = panel.GetText();
            }

            foreach (var panel in debugPanel)
            {
                if (panel.ContentType != ContentType.TEXT_AND_IMAGE) panel.ContentType = ContentType.TEXT_AND_IMAGE;
                panel.FontSize = 0.55f;
                panel.Font = "LoadingScreen";
                panel.WriteText('[' + DateTime.Now.ToString() + ']', false);
                panel.WriteText(" ", true);
                panel.WriteText(text, true);
                panel.WriteText("\n", true);
                panel.WriteText(temp, true);
            }
        }

        public string AmountUnitConversion(double amount)
        {
            double temp = 0;
            string result = "";

            if (amount >= 1000000000000000)
            {
                temp = Math.Round(amount / 1000000000000000, 1);
                result = temp.ToString() + "KT";
            }
            else if (amount >= 1000000000000)
            {
                temp = Math.Round(amount / 1000000000000, 1);
                result = temp.ToString() + "T";
            }
            else if (amount >= 1000000000)
            {
                temp = Math.Round(amount / 1000000000, 1);
                result = temp.ToString() + "G";
            }
            else if (amount >= 1000000)
            {
                temp = Math.Round(amount / 1000000, 1);
                result = temp.ToString() + "M";
            }
            else if (amount >= 1000)
            {
                temp = Math.Round(amount / 1000, 1);
                result = temp.ToString() + "K";
            }
            else
            {
                temp = Math.Round(amount, 1);
                result = temp.ToString();
            }

            return result;
        }

        public void ReloadGoodsLcdGoodsList() {
            Echo("ReloadGoodsLcdGoodsList");

            BuildGoodsList();

            goodsListLcd.ContentType = ContentType.TEXT_AND_IMAGE;
            goodsListLcd.FontSize = 0.75F;
            goodsListLcd.FontColor = Color.SkyBlue;
            goodsListLcd.Alignment = TextAlignment.CENTER;

            currentPageGoods.Clear();
            goodsListLcd.WriteText("", false);

            goodsListLcd.WriteText("商品列表\n 总项数[" + goodsList.Count + "] 当前页[" + page + "/" + (((goodsList.Count - 1) / size) + 1) + "] \n", false);
            goodsListLcd.WriteText("原料=>产物-----兑换比例-----库存\n", true);

            int first = (page - 1) * size;
            int last = (first + size) > goodsList.Count ? goodsList.Count : (first + size);


            string log = "first: " + first + " last: " + last;
            if (selectGoods.Name != null && selectGoods.Name.Length > 0) {
                log = log + "\n current: " + TranslateName(selectGoods.Name);
            }
            DebugLCD(log);

            for (int i = first; i < last; i++)
            {
                currentPageGoods.Add(goodsList[i]);
            }

            if (selectGoods.Name == "" && currentPageGoods.Count > 0)
            {
                UpdateSelectGoods(currentPageGoods[0]);
            }

            foreach (var g in currentPageGoods)
            {
                if (g.Name == selectGoods.Name)
                {
                    goodsListLcd.WriteText("> ", true);
                }
                else
                {
                    goodsListLcd.WriteText("", true);
                }

                string source = "", target = "";
                if (g.IsSource)
                {
                    source = g.Name;
                    target = g.BarterRelation.target;
                }
                else { 
                    source = g.BarterRelation.source;
                    target = g.Name;
                }

                goodsListLcd.WriteText(TranslateName(source), true);
                goodsListLcd.WriteText("=>", true);
                goodsListLcd.WriteText(TranslateName(target), true);
                goodsListLcd.WriteText("---", true);
                goodsListLcd.WriteText(g.BarterRelation.Ratio > 1 ? "1:" + g.BarterRelation.Ratio.ToString() : (1 / g.BarterRelation.Ratio).ToString() + ":1", true);
                goodsListLcd.WriteText("---", true);
                goodsListLcd.WriteText(AmountUnitConversion(g.Amount), true);
                goodsListLcd.WriteText("\n", true);
            }


        }

        /// <summary>
        /// 光标上移
        /// </summary>
        public void UpSelectGoodsInLcd()
        {
            int index = currentPageGoods.FindIndex(g => g.Name == selectGoods.Name);
            int newIndex = 0;
            if (index > 0)
            {
                newIndex = index - 1;
            }
            UpdateSelectGoods(currentPageGoods[newIndex]);
            ReloadGoodsLcdGoodsList();
        }

        /// <summary>
        /// 光标下移
        /// </summary>
        public void DownSelectGoodsInLcd()
        {
            int index = currentPageGoods.FindIndex(g => g.Name == selectGoods.Name);
            int newIndex = 0;
            if (index == currentPageGoods.Count - 1)
            {
                newIndex = index;
            }
            else
            {
                newIndex = index + 1;
            }
            UpdateSelectGoods(currentPageGoods[newIndex]);
            ReloadGoodsLcdGoodsList();
        }


        /// <summary>
        /// 更新选择的商品
        /// </summary>
        public void UpdateSelectGoods(Goods goods)
        {
            selectGoods = goods;
        }

        /// <summary>
        /// 上一页
        /// </summary>
        public void PageUpSelectGoodsInlcd()
        {
            if (page > 1)
            {
                page--;
            }
            UpdateSelectGoods(emptyGoods);
            ReloadGoodsLcdGoodsList();
        }

        /// <summary>
        /// 下一页
        /// </summary>
        public void PageDownSelectGoodsInlcd()
        {
            int lastPage = (((goodsList.Count - 1) / size) + 1);
            if (page < lastPage)
            {
                page++;
            }
            UpdateSelectGoods(emptyGoods);
            ReloadGoodsLcdGoodsList();
        }

        /// <summary>
        /// 提交物品交换，比例按source为1时计算。
        /// source / target = ratio 
        /// </summary>
        public void SubmitGoodsBarter() {
            if (selectGoods.Name == null || selectGoods.Name.Length == 0 || selectGoods.IsSource) { return; }
            string source = selectGoods.BarterRelation.source;
            string target = selectGoods.BarterRelation.target;
            double ratio = selectGoods.BarterRelation.Ratio;
            double maxCanChangeSourceAmount = (long)(selectGoods.Amount / ratio); // 站点关于该target最大能兑换的source数量

            // 先找到source和target的数量
            long targetAmount = GetTargetAmountFromStockCargos(target);
            long sourceAmount = GetSourceAmountFromTradeCargos(source);

            // 获取两者的剩余空间是否足够
            //long freeSpaceForStockCargos = GetFreeSpaceForStockCargos();
            //long freeSpaceForTradeCargos = GetFreeSpaceForTradeCargos();

            double moveSourceAmount = Math.Min(sourceAmount, maxCanChangeSourceAmount); // 在交易箱子里的总数，和站点关于该target最大能兑换的source数量，之间取最小值。
            //moveSourceAmount = Math.Min(moveSourceAmount, freeSpaceForStockCargos);
            double moveTargetAmount = (long)(moveSourceAmount * ratio); // 根据可移动的source数量计算需要移动target的数量

            //if (moveTargetAmount > freeSpaceForTradeCargos)
            //{   // 交易箱子空间不足，此时则根据交易箱子的剩余空间设定target的移动量，并重新计算source的移动量
            //    moveTargetAmount = freeSpaceForTradeCargos;
            //    moveSourceAmount = (long)(moveTargetAmount * ratio);
            //}

            if (moveTargetAmount == 0 || moveSourceAmount == 0) { return; }
            string resLog = "moveSourceAmount=" + moveSourceAmount + " moveTargetAmount=" + moveTargetAmount + "\n";

            // 根据计算好的移动量进行移动
            string resLog1, resLog2;
            MoveSouceFromTradeToStockCargos(source, moveSourceAmount, out resLog1);
            MoveTargetFromStockToTrade(target, moveTargetAmount, out resLog2);
            resLog += resLog1;
            resLog += resLog2;

            DebugLCD(resLog);
            //ReloadGoodsLcdGoodsList();
        }

        public void MoveSouceFromTradeToStockCargos(string source, double moveSourceAmount, out string resLog)
        {
            var needMoveTotal = moveSourceAmount;
            resLog = "start needMoveTotal: " + needMoveTotal + "\n";
            foreach (var c in tradeCargos)
            {
                if (needMoveTotal <= 0) { continue; }
                IMyInventory fromInventory = c.GetInventory();
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                fromInventory.GetItems(items);
                var index = items.FindIndex((i) => i.Type.ToString() == source);
                if (index < 0) continue;
                var item = items[index];
                double itemAmount = item.Amount.ToIntSafe();
                double moveAmount = Math.Min(itemAmount, needMoveTotal);
                if (moveAmount <= 0) { continue; }
                foreach (var c2 in stockCargos)
                {
                    if (itemAmount <= 0 || needMoveTotal <= 0) { continue; }
                    moveAmount = Math.Min(moveAmount, needMoveTotal);
                    IMyInventory toInventory = c2.GetInventory();
                    bool success = toInventory.TransferItemFrom(fromInventory, item, MyFixedPoint.DeserializeString((moveAmount).ToString()));
                    resLog += "[" + (success ? "Success" : "Fail") + "]";
                    if (success) {
                        itemAmount -= moveAmount;
                        needMoveTotal -= moveAmount; 
                    }
                    resLog += "[" + c.DisplayNameText + " => " + c2.DisplayNameText + "] ";
                    resLog += "moveAmount: " + moveAmount + "; needMoveTotal: " + needMoveTotal;
                    resLog += "\n";
                }
            }
        }
        public void MoveTargetFromStockToTrade(string target, double moveTargetAmount, out string resLog) {
            double needMoveTotal = moveTargetAmount;
            resLog = "start needMoveTotal: " + needMoveTotal + "\n";
            foreach (var c in stockCargos)
            {
                if (needMoveTotal <= 0) { continue; }
                IMyInventory fromInventory = c.GetInventory();
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                fromInventory.GetItems(items);
                var index = items.FindIndex((i) => i.Type.ToString() == target);
                if (index < 0) continue;
                var item = items[index];
                double itemAmount = item.Amount.ToIntSafe();
                double moveAmount = Math.Min(itemAmount, needMoveTotal);
                if (moveAmount <= 0) { continue; }
                foreach (var c2 in tradeCargos)
                {
                    if (itemAmount <= 0 || needMoveTotal <= 0) { continue; }
                    moveAmount = Math.Min(moveAmount, needMoveTotal);
                    IMyInventory toInventory = c2.GetInventory();
                    bool success = toInventory.TransferItemFrom(fromInventory, item, MyFixedPoint.DeserializeString((moveAmount).ToString()));
                    resLog +=  "[" + (success ? "Success" : "Fail") + "]";
                    if (success)
                    {
                        itemAmount -= moveAmount;
                        needMoveTotal -= moveAmount;
                    }
                    resLog += "[" + c.DisplayNameText + " => " + c2.DisplayNameText + "] ";
                    resLog += "moveAmount: " + moveAmount + "; needMoveTotal: " + needMoveTotal;
                    resLog += "\n";
                }
            }

        }

        public double GetFreeSpaceForStockCargos() {
            double result = 0;
            foreach (var c in stockCargos) {
                result += (c.GetInventory().MaxVolume.RawValue - c.GetInventory().CurrentVolume.RawValue);
            }
            return result;
        }

        public double GetFreeSpaceForTradeCargos()
        {
            double result = 0;
            foreach (var c in tradeCargos)
            {
                result += (c.GetInventory().MaxVolume.RawValue - c.GetInventory().CurrentVolume.RawValue);
            }
            return result;
        }


        /// <summary>
        /// 从所有存货箱统计target的数量
        /// </summary>
        public long GetTargetAmountFromStockCargos(string target) {
            long result = 0;
            foreach (var c in stockCargos) {
                var strs = target.Split('/');
                result += c.GetInventory().GetItemAmount(new MyItemType(strs[0], strs[1])).ToIntSafe();
            }
            return result;
        }

        /// <summary>
        /// 从交易箱子统计source数量
        /// </summary>
        public long GetSourceAmountFromTradeCargos(string source) {
            long result = 0;
            foreach (var c in tradeCargos)
            {
                var strs = source.Split('/');
                result += c.GetInventory().GetItemAmount(new MyItemType(strs[0], strs[1])).ToIntSafe();
            }
            return result;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Echo($"{DateTime.Now}");

            ProgrammableBlockScreen();

            DebugLCD("arg: " + argument);
            if ("ItemSelectDown" == argument)
            {
                DownSelectGoodsInLcd();
            }
            if ("ItemSelectUp" == argument)
            {
                UpSelectGoodsInLcd();
            }

            if ("ItemSelectPageUp" == argument)
            {
                PageUpSelectGoodsInlcd();
            }

            if ("ItemSelectPageDown" == argument)
            {
                PageDownSelectGoodsInlcd();
            }

            if ("Submit" == argument)
            {
                SubmitGoodsBarter();
            }



        }
    }
}
