﻿using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

//若要开始生成可本地化的应用程序，请在
//<PropertyGroup> 中的 .csproj 文件中
//设置 <UICulture>CultureYouAreCodingWith</UICulture>。例如，如果您在源文件中
//使用的是美国英语，请将 <UICulture> 设置为 en-US。然后取消
//对以下 NeutralResourceLanguage 特性的注释。更新
//以下行中的“en-US”以匹配项目文件中的 UICulture 设置。

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //主题特定资源词典所处位置
                                     //(在页面、应用程序或任何主题特定资源词典中
                                     // 未找到某个资源的情况下使用)
    ResourceDictionaryLocation.SourceAssembly //常规资源词典所处位置
                                              //(在页面、应用程序或任何主题特定资源词典中
                                              // 未找到某个资源的情况下使用)
)]
[assembly: XmlnsDefinition("http://WenheInfo.com/wpf/xaml/presentation/WpfAppControl", "WpfAppControl")]
[assembly: NeutralResourcesLanguage("zh-CN")]