# barter

# ���ʹ��

�������ַ [Program.cs](Program.cs)

���ƴ��뵽SE�ı�̿���

�������������д�����

��ʼ��
![image](https://github.com/se-scripts/inventory-graphic/assets/46225881/c9da6269-6c71-4e49-b25e-9e928ebe86c4)

������
![image](https://github.com/se-scripts/inventory-graphic/assets/46225881/6740f7e2-f7e6-4f36-ab58-08f4d856180e)

# ����

## Cargo | ����
��Ҫ�Ѷ�Ӧ���ӵ�`�Զ�������`���ó�����ֵ��
- PublicItemCargo���������ӣ������̵�Ľ������ӣ��˿Ϳ��Է��ʣ���Ҫ�Լ����ó�`�������˹���`��
- PrivateItemCargo���̵�Ļ������ӣ��洢�̵�Ļ��

## ��ť��̿���������

��ť�����������ö�����ѡ���̿飬���в����������·��Ĳ�����

- ItemSelectUp: ��һ��
- ItemSelectDown: ��һ��
- ItemSelectPageDown: ��һҳ
- ItemSelectPageUp: ��һҳ
- Submit: �ύ

## �Զ�������

�Զ��������޸ĺ󣬾���Ҫ���ô��룡����


## �һ������б� [[BarterConfig]]

- `Length`: �б��ж����о��Ƕ���
- ��ʽ��ԭ��ID:����ID:����
- ID��β鿴: ��[���ؽű�](https://github.com/se-scripts/mybase)ʱ���������ͼ�λ���尴F�򿪣�������ƷID�б�
- ����������������Ϊ1ʱ���㣬����һ�1��������Ҫ����ԭ��

���ӣ�

```
Length=3
1=Source:Target:10
2=MyObjectBuilder_Ore/Iron:MyObjectBuilder_Ingot/Iron:100
3=MyObjectBuilder_Ore/Gold:MyObjectBuilder_Ingot/Gold:20
```

`MyObjectBuilder_Ore/Iron`������ʯ��ID��`MyObjectBuilder_Ingot/Iron`��������ID�������`100`�Ǳ���������һ��������Ҫ100������ʯ��

