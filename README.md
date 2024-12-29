# barter

# 如何使用

进这个网址 [Program.cs](Program.cs)

复制代码到SE的编程块里

区域是上下两行大括号

开始行
![image](https://github.com/se-scripts/inventory-graphic/assets/46225881/c9da6269-6c71-4e49-b25e-9e928ebe86c4)

结束行
![image](https://github.com/se-scripts/inventory-graphic/assets/46225881/6740f7e2-f7e6-4f36-ab58-08f4d856180e)

# 配置

## Cargo | 箱子
需要把对应箱子的`自定义数据`设置成如下值：
- PublicItemCargo：公共箱子，就是商店的交易箱子，顾客可以访问，需要自己配置成`与所有人共享`。
- PrivateItemCargo：商店的货物箱子，存储商店的货物。

## 按钮编程块命令配置

按钮工具栏，设置动作，选择编程块，运行参数，填入下方的参数：

- ItemSelectUp: 上一个
- ItemSelectDown: 下一个
- ItemSelectPageDown: 下一页
- ItemSelectPageUp: 上一页
- Submit: 提交

## 自定义数据

自定义数据修改后，均需要重置代码！！！


## 兑换比例列表 [[BarterConfig]]

- `Length`: 列表有多少行就是多少
- 格式：原料ID:产物ID:比例
- ID如何查看: 用[基地脚本](https://github.com/se-scripts/mybase)时，对着面板图形化面板按F打开，会有物品ID列表
- 比例：比例按产物为1时计算，代表兑换1个产物需要多少原料

例子：

```
Length=3
1=Source:Target:10
2=MyObjectBuilder_Ore/Iron:MyObjectBuilder_Ingot/Iron:100
3=MyObjectBuilder_Ore/Gold:MyObjectBuilder_Ingot/Gold:20
```

`MyObjectBuilder_Ore/Iron`是铁矿石的ID，`MyObjectBuilder_Ingot/Iron`是铁锭的ID，后面的`100`是比例，代表换一个铁锭需要100个铁矿石。

