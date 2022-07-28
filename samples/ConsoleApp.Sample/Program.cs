// See https://aka.ms/new-console-template for more information

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Luck.Walnut.Api.Protos;

var channel = GrpcChannel.ForAddress("http://localhost:5264");
var catClient = new LuCat.LuCatClient(channel);
//获取猫总数
var catCount = await catClient.CountAsync(new Empty());
Console.WriteLine($"一共{catCount.Count}只猫。");
var rand = new Random(DateTime.Now.Millisecond);

var bathCat = catClient.BathTheCat();
//定义接收吸猫响应逻辑
var bathCatRespTask = Task.Run(async() =>
{
    await foreach (var resp in bathCat.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(resp.Message);
    }
});
//随机给10个猫洗澡
for (int i = 0; i < 10; i++)
{
    await bathCat.RequestStream.WriteAsync(new BathTheCatReq() {Id = rand.Next(0, catCount.Count)});
}
//发送完毕
await bathCat.RequestStream.CompleteAsync();
Console.WriteLine("客户端已发送完10个需要洗澡的猫id");
Console.WriteLine("接收洗澡结果：");
//开始接收响应
await bathCatRespTask;

while(true)
{
    Thread.Sleep(10000);
}

Console.WriteLine("洗澡完毕");




Console.ReadLine();