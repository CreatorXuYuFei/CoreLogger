# CoreLogger 自研高性能日志插件
基于 .NET 原生日志接口实现的自定义日志组件，专为 ASP.NET Core 项目深度优化，
解决传统自定义日志 **启动阻塞 Swagger、进程 GC 闪退、文件 IO 冲突、高并发内存泄漏** 等问题。

[TOC]

## ✨ 功能特性
- ✅ 完全兼容 .NET 标准 `ILogger` / `ILoggerProvider` 接口，无缝对接原有业务代码
- ✅ 异步队列消费日志，非阻塞 HTTP 主线程，高并发场景性能优异
- ✅ 双输出终端：控制台输出 + 本地日志文件输出
- ✅ 智能文件管理：支持按大小/周期自动滚动、旧日志清理、GZip 压缩备份
- ✅ 内置敏感信息脱敏：默认支持手机号、身份证、邮箱，可自定义正则规则
- ✅ 多维度日志过滤：日志级别、模块黑白名单、关键字屏蔽
- ✅ 支持日志 `Scope` 作用域，实现 TraceId、用户ID、操作链路追踪
- ✅ 双配置模式：代码动态配置 + `appsettings.json` 配置文件配置
- ✅ DI 全局单例托管生命周期，彻底避免配置对象被 GC 回收导致进程闪退

## 🎯 运行环境与依赖
### 1. 适配框架 & 项目类型
- 运行框架：.NET 6 / .NET 7 / .NET 8
- 支持项目：ASP.NET Core WebAPI、MVC、控制台应用、Windows 服务

### 2. 项目依赖
组件基于 .NET 原生类库开发，**无需安装任何第三方 NuGet 包**，仅依赖框架内置库：
- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Configuration`
- `System.IO.Compression`

### 3. 项目引入方式
1. **源码引用**：将 `CoreLogger` 类库添加到解决方案，主项目添加「项目引用」
2. **NuGet 引用**：组件打包后，在 NuGet 包管理器搜索 `CoreLogger` 完成安装

## ⚠️ 重要前置约束（必看）
> [!WARNING]
> **Web 项目强制禁止**调用 `builder.AddProvider()` 接入 .NET 全局日志管线。
> 该操作会在应用启动早期执行重型后台任务，抢占系统资源，直接造成 **Swagger 无法访问、路由卡死、项目启动阻塞**。
> 仅通过 DI 单例注册即可正常使用全部日志功能。

## 🚀 快速接入
### 1. 引入命名空间
在 `Program.cs` 文件头部引入以下命名空间：
```cs
using CoreLogger.Extensions;
using CoreLogger.Entiy;
using Microsoft.Extensions.Logging;
