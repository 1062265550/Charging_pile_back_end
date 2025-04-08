## TCP 通信协议

2022.07.09

(V1.2)

声明：协议将持续更新,我们不另行通知,需要最新资料,请直接跟对接人沟通

<!-- Media -->

<table><tr><td>更新时间</td><td>更新版本</td><td>更新内容</td><td>更新人</td></tr><tr><td>2022.07.09</td><td>V0.3</td><td/><td>Peter</td></tr><tr><td>2022.08.01</td><td>V0.4</td><td>1、修改部分笔误； 2、增加消息例子 3、增加远程升级命令</td><td>Peter</td></tr><tr><td>2022.12.01</td><td>V0.5</td><td>1、增加电表数据读取 2、增加钱箱打开上报</td><td>Peter</td></tr><tr><td>2023.02.01</td><td>V0.6</td><td>1、增加了远程更改 IP 端口功能 2、增加充满自停开关,开启插座等 待时间,和移除插座等待时间 3、增加了卡类型设置</td><td>Peter</td></tr><tr><td>2023.08.01</td><td>V0.7</td><td>1、增加了移除功率</td><td>Peter</td></tr><tr><td>2023.12.01</td><td>V0.8</td><td>1 增加刷卡包月报天数</td><td>Peter</td></tr><tr><td>2024.09.04</td><td>V1.0</td><td>1、兼容之前数据格式下,新增每条 数据带设备 IMEI； 2、增加电量计费峰深值命令 3、増加 0x8D 0x8E 0x8F 命令 4、合并 MQTT 文档</td><td>Peter</td></tr><tr><td>2024.09.18</td><td>V1.1</td><td>1、修改电量精度为 0.001 度,</td><td>Peter</td></tr><tr><td>2024.10.10</td><td>V1.2</td><td>1.增加 C0 命令,定时上报什么</td><td>Peter</td></tr></table>

<!-- Media -->

## MQTT 主题定义

模块到服务器：JUY/D2S/IMEI/CMD/DEV

服务器到模块：JUY /S2D/IMEI/CMD/SERVER

IMEI：为模块的 15 位 ASCII 码

CMD：为通信命令字段,根据此字段判断消息

## 1、通信数据结构

帧头：双字节 0x5AA5

数据长度 LEN : 双字节,数据包长度,从 CMD 到 SUM 校验和包含 CMD 和 SUM

CMD：单字节,命令字节。

RESULT : 单字节,表示命令是否成功。0x01,成功；0x00,失败；0xFF,无网络。

<font color="green">设备编号(IMEI)：15字节 设备编号,默认是IMEI,后期有改动再更新 </font>

DATA : 数据字节,不定长

SUM：单字节,从LEN到DATA结束所有字节相加得到的值取低8位

**注意事项：**

1、多字节顺序,低位在前,高位在后,比如 0x12345678 ,在通讯协议中,0x78,0x56,0x34,0x12

2、设备IMEI在以前的版本是不存在的,只有新的软件版本才会存在,81命令的信号值改为协议版本,协议版本从0x64开始,如果小于0x64,说明不是最新版本,81命令回复登陆结果0xF0也无法升级协议命令,也无法通过81命令的信号值,来判定协议版本,且81命令不存在设备编号数据字段；当81命令回复登陆结果为0xF0,设备就会按照新的协议格式发送数据,设备断网或者重连后,继续按照以上流程配置；

1、只有 TCP 协议数据才在数据中带 IMEI,MQTT 主题已经可以区分；

4、0x81 命令发送和接收都不能跟之前版本一样；

5、0x8F 命令只有在尖峰低谷电量模式启用后,才会上传,且上传此命令就不会传 0x88 命令

6、0x85结束命令如果是波峰平谷,档位数固定5段,之前的档位时间就是各时段耗电量和使用金额

7、设备上电不发送8E命令,则跟之前计费逻辑一样,设置后才进入波峰平谷计费





## 2、命令集

<table><tr><td>名称</td><td>命令CMD</td><td>说明</td></tr><tr><td>设备登陆</td><td>0x81</td><td/></tr><tr><td>心跳数据</td><td>0x82</td><td/></tr><tr><td>远程启动</td><td>0x83</td><td/></tr><tr><td>远程停止</td><td>0x84</td><td/></tr><tr><td>提交充电结束 状态</td><td>0x85</td><td/></tr><tr><td>本地启动上报</td><td>0x86</td><td/></tr><tr><td>在线卡信息</td><td>0x87</td><td/></tr><tr><td>查询端口数据</td><td>0x88</td><td/></tr><tr><td>查询设备参数 表</td><td>0x89</td><td/></tr><tr><td>设置设备参数</td><td>0x8A</td><td/></tr><tr><td>远程更改IP</td><td>0x8C</td><td/></tr><tr><td>设置尖峰平谷 费率</td><td>0x8D</td><td>TC系列 M2.0以上支持</td></tr><tr><td>查询尖峰平谷 费率</td><td>0x8E</td><td>TC系列 M2.0以上支持</td></tr><tr><td>查询端口电量数据</td><td>0x8F</td><td>只有在电量波峰平谷启用的状态下,才发送0x8F,发送此命令就不会发送 0x88 命令</td></tr><tr><td>定时上报IMEI</td><td>0xC0</td><td>定时上报IMEI</td></tr><tr><td>远程升级</td><td>0xF5</td><td/></tr></table>

## 3、命令字段解释

### 3.1 设备登陆 (CMD:0x81)

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>IMEI</td><td>15</td><td>ASCII</td><td>设备 ID 即模组 IMEI 号</td></tr><tr><td>设备端口数</td><td>1</td><td>BYTE</td><td>此设备总的端口数</td></tr><tr><td>硬件版本</td><td>16</td><td>ASCII</td><td>主板硬件版本号</td></tr><tr><td>软件版本</td><td>16</td><td>ASCII</td><td>主板软件版本号</td></tr><tr><td>CCID</td><td>20</td><td>ASCII</td><td>20位CCID,<font color="red">89860390845513443049</font></td></tr><tr><td>信号值/协议版本</td><td>1</td><td>Byte</td><td>0-32/0x64 以上,<font color="red">当此字段大于等于 0x64 时,说明协议可以更新</font></td></tr><tr><td>登陆原因</td><td>1</td><td>Byte</td><td>0x00:上电登陆 0x01:软件重启 Other:后期根据需要会有其它字段,注意预 留</td></tr></table>



<font color="red">设备登陆必须响应,否则会一直发送登陆消息,不做其它数据处理<br/>原始数据:
5AA5490081003836313139373036323933343338370A4A55595F42325F513830304D5F315F304A55595F42325F434F4D4D5F56312E3738393836303445383130323343303936333733311B005F</font>

5AA5 包头
4900 数据区长度
81 CMD
00 忽略

383631313937303632393334333837 IMEI 861197062934387 ASCII码

0A 端口数 10个端口

4A55595F42325F513830304D5F315F30 硬件版本ASCII码 JUY_B2_Q800M_1_0

4A55595F42325F434F4D4D5F56312E37 软件版本ASCII码 JUY_B2_COMM_V1.7

3839383630344538313032334330393633373331 CIID 就是物联网卡号

898604E81023C0963731

1B 信号值

00 登陆原因

5F 检验值

### 设备登陆响应(CMD:0x81)：

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>时间</td><td>7</td><td>BCD</td><td>预留</td></tr><tr><td>心跳间隔</td><td>1</td><td>BYTE</td><td>10-250 秒</td></tr><tr><td>登陆结果</td><td>1</td><td>BYTE</td><td>0x00:登陆成功 0x01：非法模块 <font color="green">0xF0: 登陆成功,并转换到新协议格式</font> Other:后期根据需要会有其它字段,注意预留</td></tr></table>

5AA50C00810000000000000000000008D

5A A5 0C 00 81 00 00 00 00 00 00 00 00 00 F0 7D

### 3.2 心跳数据(CMD:0x82)：

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>信号值</td><td>1</td><td>Byte</td><td>信号值 0-32</td></tr><tr><td>温度值</td><td>1</td><td>BYTE</td><td>主板温度值</td></tr><tr><td>总端口数</td><td>1</td><td>BYTE</td><td>总的端口数</td></tr><tr><td>端口状态</td><td>N</td><td>BYTE</td><td>根据端口总数来判断N的值,如果设备是10路,那么N就等于10,如果N是12路,那么N就等于12.<br/> 0x00:空闲 0x01:使用中 0x02:保险丝熔断 0x03:继电器粘连 0x04:端口禁用</td></tr></table>

5A A5 11 00 82 1F 1E 0A 00 00 00 00 01 00 00 00 00 01 DC

带 IMEI (<font color="green">867924060525709</font>):

5A A5 21 00 82 00 <font color="green">38 36 37 39 32 34 30 36 30 35 32 35 37 30 39</font> 0E 22 0C 00 00 00 00 00 00 00 00 00 00 00 00 00 F6

### 心跳数据响应：(CMD:0X82) :

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>预留</td><td>1</td><td>Byte</td><td>0</td></tr></table>

5A A5 04 00 82 00 00 86

带 IMEI (<font color="green">867924060525709</font>):

5A A5 13 00 82 00<font color="green"> 38 36 37 39 32 34 30 36 30 35 32 35 37 30 39</font> 00 AB

### 3.3 远程启动(CMD:0X83):

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>端口</td><td>1</td><td>BYTE</td><td>需要开启的端口号：1-N</td></tr><tr><td>订单号</td><td>4</td><td>UINT32</td><td>订单号</td></tr><tr><td>启动方式</td><td>1</td><td>BYTE</td><td>1：扫码支付 2：刷卡支付 3：管理员启动</td></tr><tr><td>卡号</td><td>4</td><td>UINT32</td><td>非 IC 卡启动,字段为 0</td></tr><tr><td>充电方式</td><td>1</td><td>BYTE</td><td>1：充满自停 2：按金额 3：按时间 4：按电量 5：其它</td></tr><tr><td>充电参数</td><td>4</td><td>UINT32</td><td>**秒 0.01 元 0.01 度</td></tr><tr><td>可用金额</td><td>4</td><td>UINT32</td><td>用户剩余金额(0.01 元)</td></tr></table>

5A A5 16 00 83 00 02 01 00 00 00 01 00 00 00 00 01 E8 03 00 00 64 00 00 ED

带 IMEI (<font color="green">867924060525709</font>):

5A A5 25 00 83 00<font color="green"> 38 36 37 39 32 34 30 36 30 35 32 35 37 30 39 </font>02 01 00 00 00 01 00 00 00

00 01 E8 03 00 00 64 00 00 02

0x02 代表：端口:02

订单号：0000001

启动方式：1 扫码支付

卡号：没有卡号就填 00000000

充电方式：0x01 充满自停

充电参数：0x000003E8,1000 秒

可用金额：0x00000064,100 分 = 1 元

## 远程启动响应:

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>端口</td><td>1</td><td>BYTE</td><td>需要开启的端口号：1-N</td></tr><tr><td>订单号</td><td>4</td><td>UINT32</td><td>订单号</td></tr><tr><td>启动方式</td><td>1</td><td>BYTE</td><td>1：扫码支付 2：刷卡支付 3：管理员启动</td></tr><tr><td>启动结果</td><td>1</td><td>BYTE</td><td>0 : 成功启动 1 : 正在充电 2：端口故障</td></tr></table>

### 3.4、远程停止(0x84):

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>端口</td><td>1</td><td>Byte</td><td>需要停止的端口号：1-N</td></tr><tr><td>订单号</td><td>4</td><td>UINT32</td><td>流水号</td></tr></table>

5A A5 08 00 84 00 02 01 00 00 00 8F

带 IMEI (<font color="green">867924060525709</font>):

5A A5 17 00 84 00<font color="green"> 38 36 37 39 32 34 30 36 30 35 32 35 37 30 39</font> 02 01 00 00 00 B4

## 设备响应：

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>端口</td><td>1</td><td>BYTE</td><td>需要停止的端口号：1-N</td></tr><tr><td>订单号</td><td>4</td><td>UINT32</td><td>流水号</td></tr><tr><td>结果</td><td>1</td><td>BYTE</td><td>00：执行成功 01：端口本来就空闲 02：订单号不匹配</td></tr></table>

### 3.5、提交结束状态(CMD:0X85):

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>端口</td><td>1</td><td>BYTE</td><td>需要开启的端口号：1-N</td></tr><tr><td>订单号</td><td>4</td><td>UINT32</td><td>订单号</td></tr><tr><td>用户充电 时间</td><td>4</td><td>UINT32</td><td>用户使用充电时间秒数</td></tr><tr><td>充电电量</td><td>4</td><td>UINT32</td><td>用户使用电量 0.01 度</td></tr><tr><td>使用金额</td><td>4</td><td>UINT32</td><td>0.01 元</td></tr><tr><td>停止原因</td><td>1</td><td>BYTE</td><td>0：充满自停 1：时间用完 2：金额用完 3 : 手动停止 4 : 电量用完 5 : 端口功率过大 6 : 未检测到充电器 7：温度过大停止 8：烟雾停止 9：智能充电停止</td></tr><tr><td>停止时功 率</td><td>2</td><td>UINT16</td><td>停止时当前功率 W</td></tr><tr><td>卡号</td><td>4</td><td>UINT32</td><td>投币和免费启动忽略</td></tr><tr><td>档位数</td><td>1</td><td>BYTE</td><td>充电过程总档位数 N(电量模式：固定为5尖峰平深谷5个档位)</td></tr><tr><td>档位时间1-N</td><td>2 * N</td><td>UIN16</td><td>单位秒S 尖峰平谷电量</td></tr><tr><td>档位价格</td><td>2*N</td><td>UINT16</td><td>0.01 元(电量模式尖峰平谷金额)</td></tr><tr><td>预留</td><td>8</td><td>BYTE</td><td>扩展用</td></tr></table>

<font color="red">结算账单上传必须回复,若 10S 内未回复,设备将再次上发账单最多 3 次,3次仍然没有收到回复,将变成异常订单,平台自动结算</font>

5A A5 2C 00 85 00 01 01 00 00 00 E8 03 00 00 10 00 00 00 0A 00 00 00 00 00 00 00 00 00 00 00 00 02 F4 01 19 00 F4 01 1E 00 00 00 00 00 00 00 D4

5A A5 3B 00 84 00<font color="green"> 38 36 37 39 32 34 30 36 30 35 32 35 37 30 39</font> 01 01 00 00 00 E8 03 00 00 10 00 00 00 0A 00 00 00 00 0E 00 00 00 00 00 02 F4 01 19 00 F4 01 1E 00 00 00 00 00 00 00 00 00 00 00 EA

含义：

1 号端口

订单号 0x00000001

使用时间：0x000003E8 1000S

使用电量：0x00000010 (0.16 度)

使用金额 0x0000000A 0.1 元

停止原因：0x00 充满自停

停止时功率：0x0E 停止时功率 15W

卡号：0x00000000

充电过程的档位数：0x02 在两个档位都有充电时间

档位时间：0x32 50 秒

档位价格：0x190.25 元

档位时间：0x32 50 秒

档位价格：0x1E 0.3元

预留 : 8 位

<font color="red">注：这个结算订单包含了两种计费模式,如果按时间打折模式,那么结算使用金额时,不需要关注档位数和档位时间和价格,直接取剩余时间平台结算,如果是按小时计费,则需要参考档位个数和价格,此次使用金额为50\*0.25+50*0.3 = 0.07元</font>

## 结束状态响应:

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>端口</td><td>1</td><td>BYTE</td><td>需要开启的端口号：1-N</td></tr><tr><td>订单号</td><td>4</td><td>UINT32</td><td>订单号</td></tr></table>

### 3.6、本地启动(CMD:0X86)：

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>端口</td><td>1</td><td>BYTE</td><td>需要开启的端口号：1-N</td></tr><tr><td>订单号</td><td>4</td><td>UINT32</td><td>订单号</td></tr><tr><td>启动方式</td><td>1</td><td>BYTE</td><td>1：线下卡 2：投币 3：按键免费</td></tr><tr><td>消费金额</td><td>4</td><td>UINT32</td><td>此次消费金额 0.01 元</td></tr><tr><td>剩余金额</td><td>4</td><td>UINT32</td><td>线下卡卡上剩余金额 0.01 元 投币不管</td></tr><tr><td>卡号</td><td>4</td><td>UINT32</td><td>投币和免费启动忽略</td></tr></table>

<font color="red">本地启动信息上传必须回复,若 10S 内未回复,设备将再次上传启动订单最多 3 次,3 次仍然没有收到回复,将不再上传</font>

## 本地启动响应:

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>端口</td><td>1</td><td>BYTE</td><td>需要开启的端口号：1-N</td></tr><tr><td>订单号</td><td>4</td><td>BYTE</td><td>订单号</td></tr></table>

### 3.7、在线卡信息认证 (0x87)：

<font color="red">若是扣费状态,平台直接下发远程启动命令不再回复 0x87 命令,若为查询余额命令,则返回 x87 命令</font>

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>端口</td><td>1</td><td>BYTE</td><td>需要开启的端口号：1-N</td></tr><tr><td>卡号</td><td>4</td><td>UINT32</td><td>4 字节卡号</td></tr><tr><td>操作指令</td><td>1</td><td>BYTE</td><td>1：查询余额 2：启动充电 3：按键免费</td></tr></table>

## 在线卡信息响应:

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>端口</td><td>1</td><td>BYTE</td><td>需要开启的端口号：1-N</td></tr><tr><td>卡号</td><td>4</td><td>UINT32</td><td>卡 ID 号</td></tr><tr><td>卡余额</td><td>4</td><td>UINT32</td><td>卡账号余额/天数</td></tr><tr><td>认证结果</td><td>1</td><td>BYTE</td><td>0 : 认证通过</td></tr><tr><td/><td/><td/><td>1：非法账户</td></tr><tr><td/><td/><td/><td>2：卡号冻结</td></tr><tr><td/><td/><td/><td>5 : 余额不足 6：正在使用中</td></tr><tr><td/><td/><td/><td>7：报卡剩余天数</td></tr></table>

### 3.8 查下充电端口数据(CMD:0x88)

此数据平台可以查询,也可以当有端口工作时,按心跳间隔上报,如果所有端口空闲,将不会有数据主动上报,查询时也不会有端口信息

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>Reserved</td><td>1</td><td>BYTE</td><td>0</td></tr></table>

## 充电端口数据响应

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>充电端口 数</td><td>1</td><td>BYTE</td><td>正在使用的端口数</td></tr><tr><td>电压</td><td>2</td><td>UINT16</td><td>0.1V</td></tr><tr><td>温度</td><td>1</td><td>BYTE</td><td>设备温度</td></tr><tr><td>端口号</td><td>1</td><td>BYTE</td><td>端口编号 1-N</td></tr><tr><td>当前档位</td><td>1</td><td>BYTE</td><td>充电桩当前档位 0-7(充电过程的最大档 位)</td></tr><tr><td>当前电价</td><td>2</td><td>UINT16</td><td>0.01 元</td></tr><tr><td>当前功率</td><td>2</td><td>UINT16</td><td>瓦(端口的实时功率)</td></tr><tr><td>使用时间</td><td>4</td><td>UINT32</td><td>使用时间 S</td></tr><tr><td>使用金额</td><td>2</td><td>UINT16</td><td>0.01 元,当前消费金额</td></tr><tr><td>使用电量</td><td>4</td><td>UINT32</td><td>0.01 度</td></tr><tr><td>端口温度</td><td>1</td><td>BYTE</td><td>端口温度</td></tr></table>

如当前有 4 个端口在充电分别是 1, 5, 6, 8 号端口,这充电端口数为 4,端口号 1 档位\*\* 电价\*\* 

------端口温度\*\*,端口号 5 档位\*\*电价\*\*\* 

------端口温度\*\*端口号6 档位\*\* 电价\*\*\* ; 

------端口温度\** 端口号 8 档位\*\*电价*\*\* ------端口温度**

档位为充电过程的最大档位,比如开始在第二档,100 分钟后,在第一档,那么一直是第二档,不会跳到第一档；以最高的为准

### 3.9、查询设备参数表(CMD:0x89)

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>RESERVE</td><td>1</td><td>BYTE</td><td>0</td></tr></table>

5A A5 04 00 89 00 00 8D

## 参数表数据响应

返回参数配置表中的数据

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>参数表中的数据</td><td>1</td><td>BYTE</td><td>见设备参数表</td></tr></table>

5aa5 包头

4700 数据 长度

89 命令

00 result 没用

00 计费模式

f000 单次投币充电时间和免费充电模式单次时间

0000 单次刷卡充电时间

0a00 单次刷卡扣费金额

c800 第一档功率

2c01

f401

2003

e803

dc05

d007

c409 //功率

1400

1900

1e00

3c00

5a00

6400

9b00

ff00 //单价

Of 浮充参考功率

7800 浮充时间

00 免费模式

5a //温度预警

00 //烟雾

08 //音量

00000000 //使用电量 没有用

00 //开关量

7800 充电器插入等待时间

7800 充电器移除等待时间

00 //IC 卡类型,默认 0 就行

02 //移除功率

0000000000000000000000 //预留

01 //校验和

### 3.10、远程升级(0xF5)

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>RESERVE</td><td>1</td><td>BYTE</td><td>0</td></tr></table>

5AA50400F50000F9

## 设备响应:

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>Result</td><td>1</td><td>BYTE</td><td>0 : 操作成功 1：失败</td></tr><tr><td>当前电表 读数</td><td>4</td><td>UINT32</td><td>0.01KW.h</td></tr></table>

### 3.11、获取电表读数(0x8B)(485 主板才支持根据实际硬件查看)

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>CMD</td><td>1</td><td>BYTE</td><td>0：获取电表当前读数 1：清除电表当前电度数,清楚后,无法恢复</td></tr></table>

### 设备响应:

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>RESERVE</td><td>1</td><td>BYTE</td><td>00</td></tr></table>

### 3.12、设置设备参数(CMD:0x8A)(<font color="red">不可频繁设置</font>)

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>参数表中 的数据</td><td>1</td><td>BYTE</td><td>见设备参数表</td></tr></table>

例如：5AA547008A0000D002D00264006400640064006400640064006400640064006400640064006400640064006400203000009001090000000000000000000000000000000000000000000004

### 设置参数响应:

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>Reserved</td><td>1</td><td>BYTE</td><td>0</td></tr></table>

### 3.13、远程更改IP,端口(0x8C) (<font color="red">不可频繁设置</font>)

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>协议模式</td><td>1</td><td>BYTE</td><td>0:MQTT,1:TCP(目前只支持TCP)</td></tr><tr><td>IP 地址</td><td>18</td><td>ASCII</td><td>IP 地址如：192.168.1.1,不足的后面补 0</td></tr><tr><td>端口</td><td>6</td><td>ASCII</td><td>端口数 1833 不足的后面补 0</td></tr><tr><td>用户名</td><td>10</td><td>ASCII</td><td>用户名,10 字节限制 (MQTT 才用) 不足的后 面补 0</td></tr><tr><td>密码</td><td>10</td><td>ASCII</td><td>用户密码 10 字节限制(MQTT 才用)不足的 后面补 0</td></tr></table>

例如

5AA530008C00013132322e3131342e3132322e31373400000033393230370000000000000000000000000000000000000000000

<font color="red">注意：此次修改 IP 后,设备会重启,之前的 IP 将会覆盖,请勿随意操作,切换前,请用公司设备测试好后, 再下发, 否则出错责任自负</font>

### 设置参数响应:

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>Reserved</td><td>1</td><td>BYTE</td><td>设置成功</td></tr></table>

### 3.14、设置电量电价 0x8D(波峰平谷)(<font color="red">不可频繁设置</font>)

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>电量计费 开关</td><td>1</td><td>BYTE</td><td>0 : 不启用波峰平谷 1：启用波峰平谷 2:电量加时间服务费(此模式下,服务费就不 起作用, 时间服务费为在 8 个档位中的档位单 价)</td></tr><tr><td>尖电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>尖电服务 费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr></table>

<table><tr><td>峰电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>峰电服务 费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>平电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>平电服务 费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>谷电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>谷电服务 费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>深电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>深电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>计损比例</td><td>1</td><td>BYTE</td><td>用于误差太大,校准电量误差(目前不支持)</td></tr><tr><td>0:00-0:30</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr><tr><td>0:30-1:00</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr><tr><td>1:00-1:30</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr><tr><td>...</td><td>...</td><td>...</td><td>...</td></tr><tr><td>22:30- 23:00</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr><tr><td>23:00- 23:30</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr><tr><td>23:30- 00:00</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr></table>

5A A5 45 00 8D 00 01 3C 00 32 00 3C 00 32 00 3C 00 32 00 3C 00 32 00 00 03 03 03 03 03 03 03 03 03 03 03 03 03 03 02 02 02 02 02 02 02 02 02 02 02 02 01 01 01 01 01 01 01 01 01 01 01 01 01 01 01 01 01 01 00 00 00 00 00 00 00 00 00 00 00 00 01



带 IMEI(867924060525709) : S

5A A5 58 00 8D 00 38 36 37 39 32 34 30 36 30 35 32 35 37 30 39 01 3C 00 32 00 3C 00 32 003C 00 32 00 3C 00 32 00 32 00 32 00 00 03 03 03 03 03 03 03 03 03 03 03 03 02 02 02 02 02 02 02 02 02 02 02 02 01 01 01 01 01 01 01 01 01 01 01 01 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 B3

## 设备响应:

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>配置结果</td><td>1</td><td>BYTE</td><td>00 : 设置成功</td></tr></table>



### 3.15、查询电量电价 0x8E (波峰平谷)

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>配置结果</td><td>1</td><td>BYTE</td><td>00：设置成功</td></tr></table>

### 设备响应：

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>电量计费 开关</td><td>1</td><td>BYTE</td><td>0 : 不启用波峰平谷 1：启用波峰平谷</td></tr><tr><td>尖电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>尖电服务 费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>峰电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>峰电服务 费率</td><td>2</td><td>BYTE</td><td>单位 0.0001 元</td></tr><tr><td>平电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>平电服务 费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>谷电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>谷电服务 费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>深电费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>深电服务 费率</td><td>2</td><td>BYTE</td><td>单位 0.001 元</td></tr><tr><td>计损比例</td><td>1</td><td>BYTE</td><td>用于误差太大,校准电量误差(不支持)</td></tr><tr><td>0:00-0:30</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr><tr><td>0:30-1:00</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr><tr><td>1:00-1:30</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr><tr><td>...</td><td>...</td><td>...</td><td>...</td></tr><tr><td>22:30- 23:00</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr><tr><td>23:00- 23:30</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；4：深电段</td></tr><tr><td>23:30- 00:00</td><td>1</td><td>BYTE</td><td>0：尖电段；1：峰电段；2：峰电段；3：平电 段；</td></tr></table>

### 3.16、查下充电端口数据(CMD:0x8F)

此数据平台可以查询,也可以当有端口工作时,按心跳间隔上报,如果所有端口空闲,将不会有数据主动上报,查询时也不会有端口信息

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>Reserved</td><td>1</td><td>BYTE</td><td>0</td></tr></table>

## 充电端口数据响应

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>充电端口 数</td><td>1</td><td>BYTE</td><td>正在使用的端口数</td></tr><tr><td>电压</td><td>2</td><td>UINT16</td><td>0.1V</td></tr><tr><td>温度</td><td>1</td><td>BYTE</td><td>设备温度</td></tr><tr><td>当前电价</td><td>2</td><td>UINT16</td><td>0.001 元(服务费+电费)</td></tr><tr><td>端口号</td><td>1</td><td>BYTE</td><td>端口编号 1-N</td></tr><tr><td>当前档位</td><td>1</td><td>BYTE</td><td>波峰平谷深 0-4</td></tr><tr><td>当前功率</td><td>2</td><td>UINT16</td><td>瓦(端口的实时功率)</td></tr><tr><td>使用时间</td><td>4</td><td>UINT32</td><td>使用时间 S</td></tr><tr><td>使用金额</td><td>2</td><td>UINT16</td><td>0.001 元,当前消费金额</td></tr><tr><td>波电电量</td><td>2</td><td>UINT16</td><td>0.001 度</td></tr><tr><td>波电金额</td><td>2</td><td>UINT16</td><td>消费金额 0.001 元</td></tr><tr><td>峰电电量</td><td>2</td><td>UINT16</td><td>0.001 度</td></tr><tr><td>峰电金额</td><td>2</td><td>UINT16</td><td>消费金额 0.001 元</td></tr><tr><td>平电电量</td><td>2</td><td>UINT16</td><td>0.001 度</td></tr><tr><td>平峰金额</td><td>2</td><td>UINT16</td><td>消费金额 0.001 元</td></tr><tr><td>谷电电量</td><td>2</td><td>UINT16</td><td>0.001 度</td></tr><tr><td>谷峰金额</td><td>2</td><td>UINT16</td><td>消费金额 0.001 元</td></tr><tr><td>深电电量</td><td>2</td><td>UINT16</td><td>0.001 度</td></tr><tr><td>深峰金额</td><td>2</td><td>UINT16</td><td>消费金额 0.001 元</td></tr><tr><td>端口温度</td><td>1</td><td>BYTE</td><td>端口温度</td></tr></table>

如当前有 4 个端口在充电分别是 1,5,6,8 号端口,这充电端口数为 4 ,端口号 1 档位** 电价*** ------端口温度**,端口号 5 档位**电价** ------端口温度**端口号 6 档位**电价 *** ------端口温度** 端口号 8 档位**电价*** ------端口温度**

档位为充电过程的最大档位,比如开始在第二档,100 分钟后,在第一档,那么一直是第二档,不会跳到第一档；以最高的为准

### 3.17、主动上报 IMEI 身份(0xC0)(<font color="red">M2.1 版本才会支持</font>)

此命令登陆完成后,在心跳,开启返回,结束返回,刷卡请求之前,都会上报一次,表明自己的身份

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>SIGNAL</td><td>1</td><td>BYTE</td><td>信号值</td></tr><tr><td>IMEI</td><td>15</td><td>BYTE</td><td>IMEI 值</td></tr><tr><td>预留</td><td>10</td><td>BYTE</td><td/></tr></table>

### 设备响应:

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>RESERVE</td><td>1</td><td>BYTE</td><td>00</td></tr></table>

## 设备参数表：

<table><tr><td>名称</td><td>长度(字节)</td><td>数据类型</td><td>备注</td></tr><tr><td>计费模式</td><td>1</td><td>BYTE</td><td>0：按小时计费(默认) 1：按金额,传统比例打折模式</td></tr><tr><td>单次投币 充电时间</td><td>2</td><td>UINT16</td><td>单次投币充电时间默认 240 分钟</td></tr><tr><td>单次刷卡 充电时间</td><td>2</td><td>UINT16</td><td>单次刷卡充电时间默认 240 分钟(不支持)</td></tr><tr><td>单次刷卡 扣费金额</td><td>2</td><td>UINT16</td><td>单次刷卡扣费金额,单位分,如 1 元,值为 100 (不支持)</td></tr><tr><td>1-8 功率</td><td>2 * 8 </td><td>UINT16</td><td>8 档功率</td></tr><tr><td>1-8 档单 价/或打 折比例</td><td>2 * 8 </td><td>UINT16</td><td>档位价格或者比例,如果是传统时间打折模 式,此值代表打折比例 0-100,步长为 1</td></tr><tr><td>浮充参考 功率</td><td>1</td><td>BYTE</td><td>浮充参考功率 5-250W,默认 15W</td></tr><tr><td>浮充时间</td><td>2</td><td>UINT16</td><td>浮充时间,单位分钟(不能为 0,默认 120 分钟)</td></tr><tr><td>免费模式</td><td>1</td><td>BYTE</td><td>0 : 付费 1：免费(如果有按键,按按键就可直接启 动)</td></tr><tr><td>温度预警 阀值</td><td>1</td><td>BYTE</td><td>度默认 90 度</td></tr><tr><td>烟雾报警 开关</td><td>1</td><td>BYTE</td><td>0：烟雾报警关闭(默认) 1：烟雾报警打开</td></tr><tr><td>音量</td><td>1</td><td>BYTE</td><td>音量值：0-9(默认 8)</td></tr><tr><td>使用电量</td><td>4</td><td>UINT32</td><td>0.01 度</td></tr><tr><td>开关量</td><td>1</td><td>BYTE</td><td>Obit：充满自停开关量 0：充满自停,1：关闭充满自停</td></tr></table>

<table><tr><td>充电器等待插入时间</td><td>2</td><td>UINT16</td><td>开启端口后,等待插入时间,默认：120S,</td></tr><tr><td>充电器等待移除时间</td><td>2</td><td>UINT16</td><td>移除充电器后等待时间,默认：120S,</td></tr><tr><td>IC 卡类型</td><td>1</td><td>BYTE</td><td>0：兼容型,1：SDE 反码,ABA 正码,2：JY 是 SDE 正码 ABA 反码,3：其它,4：公卡</td></tr><tr><td>移除功率</td><td>1</td><td>BYTE</td><td>充电器移除功率 1-50W</td></tr><tr><td>预留</td><td>11</td><td>BYTE</td><td>预留 11 字节</td></tr></table>

