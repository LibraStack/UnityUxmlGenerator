# UnityUxmlGenerator

This package is part of [UnityMvvmToolkit](https://github.com/LibraStack/UnityMvvmToolkit).

![unityuxmlgenerator-github-cover](https://github.com/LibraStack/UnityUxmlGenerator/assets/28132516/c8c1ee74-ee9f-4343-a859-b2eb05bd152f)

## :open_book: Table of Contents

- [About](#pencil-about)
- [Folder Structure](#cactus-folder-structure)
- [Installation](#gear-installation)
- [How To Use](#joystick-how-to-use)
  - [UxmlElement](#uxmlelement)
  - [UxmlAttribute](#uxmlattribute)
- [Contributing](#bookmark_tabs-contributing)
  - [Discussions](#discussions)
  - [Report a bug](#report-a-bug)
  - [Request a feature](#request-a-feature)
  - [Show your support](#show-your-support)
- [License](#balance_scale-license)

## :pencil: About

The **UnityUxmlGenerator** allows you to generate `UxmlFactory` and `UxmlTraits` using `[UxmlElement]` and `[UxmlAttribute]` attributes.

```csharp
[UxmlElement]
public partial class CustomVisualElement : VisualElement
{
    [UxmlAttribute]
    private string CustomAttribute { get; set; }
}
```

## :cactus: Folder Structure

    .
    ├── src
    │   ├── UnityUxmlGenerator
    │   └── UnityUxmlGenerator.UnityPackage
    │       ...
    │       └── UnityUxmlGenerator.dll      # Auto-generated
    │
    ├── UnityUxmlGenerator.sln

## :gear: Installation

You can install **UnityUxmlGenerator** in one of the following ways:

<details><summary>1. Install via Package Manager</summary>
<br />
  
  The package is available on the [OpenUPM](https://openupm.com/packages/com.chebanovdd.unityuxmlgenerator/).

  - Open `Edit/Project Settings/Package Manager`
  - Add a new `Scoped Registry` (or edit the existing OpenUPM entry)

    ```
    Name      package.openupm.com
    URL       https://package.openupm.com
    Scope(s)  com.chebanovdd.unityuxmlgenerator
    ```
  - Open `Window/Package Manager`
  - Select `My Registries`
  - Install `UnityUxmlGenerator` package
  
</details>

<details><summary>2. Install via Git URL</summary>
<br />
  
  You can add `https://github.com/LibraStack/UnityUxmlGenerator.git?path=src/UnityUxmlGenerator.UnityPackage/Assets/Plugins/UnityUxmlGenerator` to the Package Manager.

  If you want to set a target version, UnityUxmlGenerator uses the `v*.*.*` release tag, so you can specify a version like `#v0.0.1`. For example `https://github.com/LibraStack/UnityUxmlGenerator.git?path=src/UnityUxmlGenerator.UnityPackage/Assets/Plugins/UnityUxmlGenerator#v0.0.1`.
  
</details>

## :joystick: How To Use

### UxmlElement

To create a custom control, just add the `[UxmlElement]` attribute to the custom control class definition. The custom control class must be declared as a partial class and be inherited from `VisualElement` or one of its derived classes. By default, the custom control appears in the Library tab in UI Builder.

You can use the `[UxmlAttribute]` attribute to declare that a property is associated with a `UXML` attribute.

The following example creates a custom control with multiple attributes:

```csharp
[UxmlElement]
public partial class CustomVisualElement : VisualElement
{
    [UxmlAttribute]
    private string CustomAttribute { get; set; }
    
    [UxmlAttribute("DefaultValue")]
    private string CustomAttributeWithDefaultValue { get; set; }
}
```

<details><summary><b>Generated code</b></summary>
<br />

`CustomVisualElement.UxmlFactory.g.cs`

```csharp
partial class CustomVisualElement
{
    [global::System.CodeDom.Compiler.GeneratedCode("UnityUxmlGenerator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public new class UxmlFactory : global::UnityEngine.UIElements.UxmlFactory<CustomVisualElement, UxmlTraits>
    {
    }
}
```

`CustomVisualElement.UxmlTraits.g.cs`

```csharp
partial class CustomVisualElement
{
    [global::System.CodeDom.Compiler.GeneratedCode("UnityUxmlGenerator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public new class UxmlTraits : global::UnityEngine.UIElements.VisualElement.UxmlTraits
    {
        [global::System.CodeDom.Compiler.GeneratedCode("UnityUxmlGenerator", "1.0.0.0")]
        private readonly global::UnityEngine.UIElements.UxmlStringAttributeDescription _customAttribute = new()
        {
            name = "custom-attribute",
            defaultValue = default
        };
  
        [global::System.CodeDom.Compiler.GeneratedCode("UnityUxmlGenerator", "1.0.0.0")]
        private readonly global::UnityEngine.UIElements.UxmlStringAttributeDescription _customAttributeWithDefaultValue = new()
        {
            name = "custom-attribute-with-default-value",
            defaultValue = "DefaultValue"
        };
  
        [global::System.CodeDom.Compiler.GeneratedCode("UnityUxmlGenerator", "1.0.0.0")]
        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override void Init(global::UnityEngine.UIElements.VisualElement visualElement, 
            global::UnityEngine.UIElements.IUxmlAttributes bag, 
            global::UnityEngine.UIElements.CreationContext context)
        {
            base.Init(visualElement, bag, context);
            var control = (CustomVisualElement)visualElement;
            control.CustomAttribute = _customAttribute.GetValueFromBag(bag, context);
            control.CustomAttributeWithDefaultValue = _customAttributeWithDefaultValue.GetValueFromBag(bag, context);
        }
    }
}
```

</details>

The following UXML document uses the custom control:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <CustomVisualElement custom-attribute="Hello World" custom-attribute-with-default-value="DefaultValue" />
</ui:UXML>
```

### UxmlAttribute

By default, the property name splits into lowercase words connected by hyphens. The original uppercase characters in the name are used to denote where the name should be split. For example, if the property name is `CustomAttribute`, the corresponding attribute name would be `custom-attribute`.

The following example creates a custom control with custom attributes:

```csharp
[UxmlElement]
public partial class CustomVisualElement : VisualElement
{
    [UxmlAttribute]
    private bool MyBoolValue { get; set; }

    [UxmlAttribute]
    private int MyIntValue { get; set; }

    [UxmlAttribute]
    private long MyLongValue { get; set; }

    [UxmlAttribute]
    private float MyFloatValue { get; set; }

    [UxmlAttribute]
    private double MyDoubleValue { get; set; }

    [UxmlAttribute]
    private string MyStringValue { get; set; }

    [UxmlAttribute]
    private MyEnum MyEnumValue { get; set; }

    [UxmlAttribute]
    private Color MyColorValue { get; set; }
}
```

Use the `[UxmlAttribute]` constructor to provide a default value for an attribute. Note that the provided value type and the property type must match. The only exception is for the `Color` type, where you must pass the name of the desired color.

```csharp
[UxmlElement]
public partial class CustomVisualElement : VisualElement
{
    [UxmlAttribute(69)]
    private int MyIntValue { get; set; }

    [UxmlAttribute(6.9f)]
    private float MyFloatValue { get; set; }

    [UxmlAttribute("Hello World")]
    private string MyStringValue { get; set; }

    [UxmlAttribute(MyEnum.One)]
    private MyEnum MyEnumValue { get; set; }

    [UxmlAttribute(nameof(Color.red))]
    private Color MyColorValue { get; set; }
}
```

## :bookmark_tabs: Contributing

You may contribute in several ways like creating new features, fixing bugs or improving documentation and examples.

### Discussions

Use [discussions](https://github.com/LibraStack/UnityUxmlGenerator/discussions) to have conversations and post answers without opening issues.

Discussions is a place to:
* Share ideas
* Ask questions
* Engage with other community members

### Report a bug

If you find a bug in the source code, please [create bug report](https://github.com/LibraStack/UnityUxmlGenerator/issues/new?assignees=ChebanovDD&labels=bug&template=bug_report.md&title=).

> Please browse [existing issues](https://github.com/LibraStack/UnityUxmlGenerator/issues) to see whether a bug has previously been reported.

### Request a feature

If you have an idea, or you're missing a capability that would make development easier, please [submit feature request](https://github.com/LibraStack/UnityUxmlGenerator/issues/new?assignees=ChebanovDD&labels=enhancement&template=feature_request.md&title=).

> If a similar feature request already exists, don't forget to leave a "+1" or add additional information, such as your thoughts and vision about the feature.

### Show your support

Give a :star: if this project helped you!

<a href="https://www.buymeacoffee.com/chebanovdd" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-orange.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" ></a>

## :balance_scale: License

Usage is provided under the [MIT License](LICENSE).
