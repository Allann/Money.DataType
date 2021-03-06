using System.Diagnostics.CodeAnalysis;

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project. Project-level
// suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the Code Analysis results, point to "Suppress Message", and click
// "In Suppression File". You do not need to add suppressions to this file manually.
[assembly: SuppressMessage(
    "Microsoft.Design",
    "CA2210:AssembliesShouldHaveValidStrongNames",
    Justification = "Not provide any security benefits, since code is Open Source and therefor the 'private' key would be freely available")]
[assembly: SuppressMessage(
    "Microsoft.Design",
    "CA1020:AvoidNamespacesWithFewTypes",
    Scope = "namespace",
    Target = "Core.Money.Extensions",
    Justification = "Core.Money just doesn't have many types at the moment.")]
[assembly: SuppressMessage(
    "Microsoft.Design",
    "CA1020:AvoidNamespacesWithFewTypes",
    Scope = "namespace",
    Target = "Core.Money",
    Justification = "Core.Money just doesn't have many types at the moment.")]
[assembly: SuppressMessage(
    "Microsoft.Usage",
    "CA2243:AttributeStringLiteralsShouldParseCorrectly",
    Justification = "CA2243 doens't apply to AssemblyInformationalVersion. Known bug in Code Analysis.")]
[assembly: SuppressMessage(
    "Microsoft.Design",
    "CA1020:AvoidNamespacesWithFewTypes",
    Scope = "namespace",
    Target = "Core.Money.Serialization.JsonNet",
    Justification = "Needs to be in a seperate namespace, otherwise it conflits with the JavaScriptSerializer.")]
[assembly: SuppressMessage(
    "StyleCop.CSharp.LayoutRules",
    "SA1503:BracesMustNotBeOmitted",
    Justification = "See Coding Guidelines.")]
[assembly: SuppressMessage(
    "StyleCop.CSharp.ReadabilityRules",
    "SA1101:Prefix local calls with this",
    Justification = "See Coding Guidelines.")]
[assembly: SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1633:File must have header",
    Justification = "See Coding Guidelines.")]
[assembly: SuppressMessage(
    "Usage",
    "CA1801:Review unused parameters",
    Justification = "Serialization method doesn't need context",
    Scope = "member",
    Target = "~M:Core.Money.Currency.#ctor(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)")]
[assembly: SuppressMessage(
    "Usage",
    "CA1801:Review unused parameters",
    Justification = "Serialization method doesn't need context",
    Scope = "member",
    Target = "~M:Core.Money.Amount.#ctor(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)")]
[assembly: SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "<Pending>", Scope = "member", Target = "~M:Core.Money.CurrencyRegistry.#cctor")]
