# 基于NetCore3.1开发的微服务学习项目
## 项目构成
### User.Identity 认证服务
- 使用IdentityServer4构建的鉴权服务
- 授权方式基于自定义的6位验证码模式
### User.API 用户服务
- 使用EFCore + Mysql 进行数据持久化
- Dockerfile完成容器化
- 数据更新时使用RabbitMQ + CAP 进行发送更新事件
### Contact.API 通讯录服务
- 使用Mongdb完成持久化
- 使用CAP+RabbitMQ订阅消息,接受数据Uer.API更新通知
### Project.API 项目服务
- 使用DDD架构进行开发,将项目转为事件驱动,持久化与应用服务剥离解耦
- 使用MediatR进行领域事件传递,RabbitMq进行集成事件的传递
- CQRS进行读写分离(仅为逻辑分离,物理层面仍是同一个Mysql)
### Gateway.API 网关服务
- Ocelot作为服务网关
- 负责服务级别的初步过滤,由AllowedScopes配置
### Recommend.API 推荐服务
### 基础设施
- 服务注册由Consul进行
- 由DnsClient + Consul完成服务间调用的服务发现
- Polly负责服务间调用时的服务容错
### 资料汇总
- [.NET Core微服务架构学习与实践系列文章目录 - Edison Zhou](https://www.cnblogs.com/edisonchou/p/dotnetcore_microservice_foundation_blogs_index_final.html)
- [ASP.NET Core分布式项目实战 - jesse](http://video.jessetalk.cn/course/5)
- [ASP.NET Core 3.x 构建 RESTful API - 杨旭](https://www.bilibili.com/video/BV1XJ411q7yy)
- [Identity Server 4杨旭](https://www.bilibili.com/video/av42364337/)
- [Identity Server 4 Doc](https://identityserver4.readthedocs.io/en/latest/)
- [IdentityServer4 中文文档与实战 - 晓晨Master](https://www.cnblogs.com/stulzq/p/8119928.html)
- [.NET Core开源API网关 – Ocelot中文文档 - 腾飞（Jesse）](https://www.cnblogs.com/jesse2013/p/net-core-apigateway-ocelot-docs.html)
- [Polly github wiki](https://github.com/App-vNext/Polly/wiki)
- [服务容错模式 - 美团技术团队](https://tech.meituan.com/2016/11/11/service-fault-tolerant-pattern.html)
- [容错限流原理 - 杨波](https://time.geekbang.org/course/detail/100007001-17782)
- [RabbitMq](https://zhuanlan.zhihu.com/p/63700605) 
- [RabbitMq模拟器](http://tryrabbitmq.com/)
- [CAP - 杨晓东](https://cap.dotnetcore.xyz/user-guide/zh/getting-started/quick-start/)
- [DDD理论学习系列——案例及目录 - 圣杰](https://www.cnblogs.com/sheng-jie/p/6931646.html)
- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [Linux + Docker + SpringCloud - 鲁斯菲尔](https://www.bilibili.com/video/BV1et411T7Rt)
- [docker documentation](https://docs.docker.com/)
