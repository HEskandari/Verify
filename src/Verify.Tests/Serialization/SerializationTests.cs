﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VerifyTests;
using VerifyXunit;
using Xunit;
using Xunit.Sdk;

// Non-nullable field is uninitialized.
#pragma warning disable CS8618

[UsesVerify]
public class SerializationTests
{
    static SerializationTests()
    {
        VerifierSettings.AddExtraDatetimeFormat("F");
        VerifierSettings.AddExtraDatetimeOffsetFormat("F");
    }

    [Fact]
    public async Task ShouldReUseDatetime()
    {
        #region Date

        var dateTime = DateTime.Now;
        var dateTimeOffset = DateTimeOffset.Now;
        var target = new DateTimeTarget
        {
            DateTime = dateTime,
            DateTimeNullable = dateTime,
            DateTimeString = dateTime.ToString("F"),
            DateTimeOffset = dateTimeOffset,
            DateTimeOffsetNullable = dateTimeOffset,
            DateTimeOffsetString = dateTimeOffset.ToString("F"),
        };

        await Verifier.Verify(target);

        #endregion
    }

    [Fact]
    public Task ShouldScrubDatetime()
    {
        var dateTime = DateTime.Now;
        var dateTimeOffset = DateTimeOffset.Now;
        var target = new DateTimeTarget
        {
            DateTime = dateTime,
            DateTimeNullable = dateTime.AddDays(1),
            DateTimeString = dateTime.AddDays(2).ToString("F"),
            DateTimeOffset = dateTimeOffset,
            DateTimeOffsetNullable = dateTimeOffset.AddDays(1),
            DateTimeOffsetString = dateTimeOffset.AddDays(2).ToString("F"),
        };

        return Verifier.Verify(target);
    }

    [Fact]
    public Task ScrubberWithBadNewLine()
    {
        var settings = new VerifySettings();
        settings.AddScrubber(s =>
        {
            s.AppendLine("b");
            s.AppendLine("c");
        });
        return Verifier.Verify("a", settings);
    }

    [Fact]
    public Task ExampleNonDefaults()
    {
        var person = new Person
        {
            Id = new Guid("ebced679-45d3-4653-8791-3d969c4a986c"),
            Title = Title.Mr,
            GivenNames = "John",
            FamilyName = "Smith",
            Spouse = "Jill",
            Children = new List<string> {"Sam", "Mary"},
            Address = new Address
            {
                Street = "1 Puddle Lane",
                Country = "USA"
            }
        };

        var settings = new VerifySettings();
        settings.ModifySerialization(_ =>
        {
            _.DontScrubDateTimes();
            _.DontIgnoreFalse();
            _.DontScrubGuids();
            _.DontIgnoreEmptyCollections();
        });
        settings.AddScrubber(s => s.Replace("Lane", "Street"));
        return Verifier.Verify(person, settings);
    }

    [Fact]
    public Task TypeNameHandlingAll()
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            Title = Title.Mr,
            GivenNames = "John",
            FamilyName = "Smith",
            Spouse = "Jill",
            Children = new List<string> {"Sam", "Mary"},
            Address = new Address
            {
                Street = "1 Puddle Lane",
                Country = "USA"
            }
        };

        var settings = new VerifySettings();
        settings.AddExtraSettings(_ => { _.TypeNameHandling = TypeNameHandling.All; });
        return Verifier.Verify(person, settings);
    }

    public class DateTimeTarget
    {
        public DateTime DateTime;
        public DateTime? DateTimeNullable;
        public DateTimeOffset DateTimeOffset;
        public DateTimeOffset? DateTimeOffsetNullable;
        public string DateTimeString;
        public string DateTimeOffsetString;
    }

    [Fact]
    public Task ShouldIgnoreDatetimeDefaults()
    {
        var target = new DateTimeTarget();

        return Verifier.Verify(target);
    }

    [Fact]
    public async Task VerifyBytes()
    {
        var settings = new VerifySettings();
        settings.UseExtension("jpg");
        await Verifier.Verify(File.ReadAllBytes("sample.jpg"), settings);
    }

    [Fact]
    public Task ShouldNotScrubInlineGuidsByDefault()
    {
        var id = new Guid("ebced679-45d3-4653-8791-3d969c4a986c");
        var product = new
        {
            Title = $"item {id} - (ID={{{id}}})",
            Variant = new
            {
                Id = "variant id: " + id
            }
        };

        return Verifier.Verify(product);
    }

    [Fact]
    public Task ShouldBeAbleToExcludeInlineGuids()
    {
        var id = Guid.NewGuid();
        var product = new
        {
            Title = $"item {id} - (ID={{{id}}})",
            Variant = new
            {
                Id = "variant id: " + id
            }
        };

        var settings = new VerifySettings();
        settings.ModifySerialization(_ =>
        {
            _.ScrubInlineGuids();
        });
        return Verifier.Verify(product, settings);
    }

    void DontIgnoreEmptyCollections()
    {
        #region DontIgnoreEmptyCollections
        VerifierSettings.ModifySerialization(_ => _.DontIgnoreEmptyCollections());
        #endregion
    }

    void DontScrubGuids()
    {
        #region DontScrubGuids
        VerifierSettings.ModifySerialization(_ => _.DontScrubGuids());
        #endregion
    }

    void ScrubInlineGuids()
    {
        #region ScrubInlineGuids
        VerifierSettings.ModifySerialization(_ => _.ScrubInlineGuids());
        #endregion
    }

    void DontScrubDateTimes()
    {
        #region DontScrubDateTimes
        VerifierSettings.ModifySerialization(_ => _.DontScrubDateTimes());
        #endregion
    }

    void DontIgnoreFalse()
    {
        #region DontIgnoreFalse
        VerifierSettings.ModifySerialization(_ => _.DontIgnoreFalse());
        #endregion
    }

    [Fact]
    public Task NewLineEscapedInProperty()
    {
        #region NewLineEscapedInProperty
        return Verifier.Verify(new {Property = "a\r\nb\\nc"});
        #endregion
    }

    [Fact]
    public async Task NewLineNotEscapedInProperty()
    {
        #region DisableNewLineEscaping
        var settings = new VerifySettings();
        settings.DisableNewLineEscaping();
        await Verifier.Verify(new {Property = "a\r\nb\\nc"}, settings);
        #endregion
    }

    void List()
    {
        var verifySettings = new VerifySettings();

        #region ScrubLines

        verifySettings.ScrubLines(line => line.Contains("text"));

        #endregion

        #region ScrubLinesContaining

        verifySettings.ScrubLinesContaining("text1", "text2");

        #endregion

        #region ScrubLinesContainingOrdinal

        verifySettings.ScrubLinesContaining(StringComparison.Ordinal, "text1", "text2");

        #endregion

        #region ScrubLinesWithReplace

        verifySettings.ScrubLinesWithReplace(line => line.ToUpper());

        #endregion

        #region ScrubMachineName

        verifySettings.ScrubMachineName();

        #endregion

        #region AddScrubber

        verifySettings.AddScrubber(fullText => fullText.Remove(0, 100));

        #endregion
    }

    [Fact]
    public Task ScrubTempPath()
    {
        return Verifier.Verify(Path.GetTempPath().TrimEnd('/', '\\'));
    }

    [Fact]
    public Task ScrubCurrentDirectory()
    {
        return Verifier.Verify(Environment.CurrentDirectory.TrimEnd('/', '\\'));
    }

    [Fact]
    public Task ScrubCodeBaseLocation()
    {
        return Verifier.Verify(CodeBaseLocation.CurrentDirectory!.TrimEnd('/', '\\'));
    }

    [Fact]
    public Task ScrubBaseDirectory()
    {
        return Verifier.Verify(AppDomain.CurrentDomain.BaseDirectory!.TrimEnd('/', '\\'));
    }

    public class TypeTarget
    {
        public Type Type;
        public Type Dynamic;
    }

    [Fact]
    public async Task ShouldUseShortTypeName()
    {
        #region type

        var foo = new {x = 1};
        var target = new TypeTarget
        {
            Type = GetType(),
            Dynamic = foo.GetType(),
        };

        await Verifier.Verify(target);

        #endregion
    }

#if(!NETSTANDARD2_0)
    [Fact]
    public async Task NamedTuple()
    {
        #region VerifyTuple

        await Verifier.Verify(() => MethodWithNamedTuple());

        #endregion
    }

    #region MethodWithNamedTuple

    static (bool Member1, string Member2, string Member3) MethodWithNamedTuple()
    {
        return (true, "A", "B");
    }

    #endregion

    [Fact]
    public async Task PartialNamedTuple()
    {
        var exception = await Assert.ThrowsAsync<Exception>(() => Verifier.Verify(() => MethodWithPartialNamedTuple()));
        await Verifier.Verify(exception.Message);
    }

    static (bool, string Member2, string Member3) MethodWithPartialNamedTuple()
    {
        return (true, "A", "B");
    }

    [Fact]
    public async Task NamedTupleWithNull()
    {
        await Verifier.Verify(() => MethodWithNamedTupleWithNull());
    }

    static (string Member1, string? Member2) MethodWithNamedTupleWithNull()
    {
        return ("A", null);
    }

    [Fact]
    public async Task ShouldReUseGuid()
    {
        #region guid

        var guid = Guid.NewGuid();
        var target = new GuidTarget
        {
            Guid = guid,
            GuidNullable = guid,
            GuidString = guid.ToString(),
            OtherGuid = Guid.NewGuid(),
        };

        await Verifier.Verify(target);

        #endregion
    }

    [Fact]
    public Task ShouldIgnoreGuidDefaults()
    {
        var target = new GuidTarget();
        return Verifier.Verify(target);
    }

    [Fact]
    public Task ShouldScrubGuid()
    {
        var target = new GuidTarget
        {
            Guid = Guid.NewGuid(),
            GuidNullable = Guid.NewGuid(),
            GuidString = Guid.NewGuid().ToString(),
        };
        return Verifier.Verify(target);
    }

    public class GuidTarget
    {
        public Guid Guid;
        public Guid? GuidNullable;
        public string GuidString;
        public Guid OtherGuid;
    }

    [Fact]
    public Task Escaping()
    {
        var target = new EscapeTarget
        {
            Property = @"\"
        };
        return Verifier.Verify(target);
    }

    public class EscapeTarget
    {
        public string Property;
    }

    [Fact]
    public Task OnlySpecificDates()
    {
        var target = new NotDatesTarget
        {
            NotDate = "1.2.3"
        };
        return Verifier.Verify(target);
    }

    public class NotDatesTarget
    {
        public string NotDate;
    }

    [Fact]
    public Task ShouldScrubDictionaryKey()
    {
        return Verifier.Verify(
            new
            {
                guid = new Dictionary<Guid, string>
                {
                    {Guid.NewGuid(), "value"}
                },
                dateTime = new Dictionary<DateTime, string>
                {
                    {DateTime.Now, "value"}
                },
                dateTimeOffset = new Dictionary<DateTimeOffset, string>
                {
                    {DateTimeOffset.Now, "value"}
                },
                type = new Dictionary<Type, string>
                {
                    {typeof(SerializationTests), "value"}
                }
            });
    }

    [Fact]
    public Task ShouldIgnoreEmptyList()
    {
        var target = new CollectionTarget
        {
            DictionaryProperty = new Dictionary<int, string>(),
            IReadOnlyDictionary = new Dictionary<int, string>(),
            ReadOnlyList = new List<string>(),
            ListProperty = new List<string>(),
            ReadOnlyCollection = new List<string>(),
            Array = new string[] { }
        };
        return Verifier.Verify(target);
    }

    public class CollectionTarget
    {
        public Dictionary<int, string> DictionaryProperty;
        public List<string> ListProperty;
        public string[] Array;
        public IReadOnlyList<string> ReadOnlyList;
        public IReadOnlyCollection<string> ReadOnlyCollection;
        public IReadOnlyDictionary<int, string> IReadOnlyDictionary;
    }
#pragma warning disable 612

    [Fact]
    public async Task ExceptionMessageProp()
    {
        #region IgnoreMembersThatThrowExpression

        var settings = new VerifySettings();
        settings.ModifySerialization(
            _ => _.IgnoreMembersThatThrow<Exception>(x => x.Message == "Ignore"));

        var target = new WithExceptionIgnoreMessage();
        await Verifier.Verify(target, settings);

        #endregion
    }

    [Fact]
    public Task ExpressionString()
    {
        var parameter = Expression.Parameter(typeof(Exception), "source");
        var property = Expression.Property(parameter, "Message");
        var convert = Expression.Convert(property, typeof(object));
        var expression = Expression.Lambda<Func<Exception, object>>(convert, parameter);
        var settings = new VerifySettings();
        settings.UniqueForRuntime();
        return Verifier.Verify(expression, settings);
    }

    class WithExceptionIgnoreMessage
    {
        public Guid ExceptionMessageProperty => throw new Exception("Ignore");
    }

    [Fact]
    public Task NotImplementedExceptionProp()
    {
        var target = new WithNotImplementedException();
        return Verifier.Verify(target);
    }

    class WithNotImplementedException
    {
        public Guid NotImplementedExceptionProperty => throw new NotImplementedException();
    }


    [Fact]
    public async Task AddIgnoreInstance()
    {
        #region AddIgnoreInstance

        var settings = new VerifySettings();
        settings.ModifySerialization(
            _ => { _.IgnoreInstance<Instance>(x => x.Property == "Ignore"); });

        var target = new IgnoreInstanceTarget
        {
            ToIgnore = new Instance
            {
                Property = "Ignore"
            },
            ToInclude = new Instance
            {
                Property = "Include"
            }
        };
        await Verifier.Verify(target, settings);

        #endregion
    }

    class IgnoreInstanceTarget
    {
        public Instance ToIgnore;
        public Instance ToInclude;
    }

    class Instance
    {
        public string Property;
    }

    [Fact]
    public async Task IgnoreType()
    {
        #region AddIgnoreType

        var settings = new VerifySettings();
        settings.ModifySerialization(_ => _.IgnoreMembersWithType<ToIgnore>());

        var target = new IgnoreTypeTarget
        {
            ToIgnore = new ToIgnore
            {
                Property = "Value"
            },
            ToInclude = new ToInclude
            {
                Property = "Value"
            }
        };
        await Verifier.Verify(target, settings);

        #endregion
    }

    class IgnoreTypeTarget
    {
        public ToIgnore ToIgnore;
        public ToInclude ToInclude;
    }

    class ToInclude
    {
        public string Property;
    }

    class ToIgnore
    {
        public string Property;
    }

    [Fact]
    public async Task IgnoreMemberByExpression()
    {
        #region IgnoreMemberByExpression

        var settings = new VerifySettings();
        settings.ModifySerialization(_ =>
        {
            _.IgnoreMember<IgnoreExplicitTarget>(x => x.Property);
            _.IgnoreMember<IgnoreExplicitTarget>(x => x.Field);
            _.IgnoreMember<IgnoreExplicitTarget>(x => x.GetOnlyProperty);
            _.IgnoreMember<IgnoreExplicitTarget>(x => x.PropertyThatThrows);
        });

        var target = new IgnoreExplicitTarget
        {
            Include = "Value",
            Field = "Value",
            Property = "Value"
        };
        await Verifier.Verify(target, settings);

        #endregion
    }

    [Fact]
    public async Task IgnoreMemberByName()
    {
        #region IgnoreMemberByName

        var settings = new VerifySettings();
        settings.ModifySerialization(_ =>
        {
            _.IgnoreMember("PropertyByName");
            var type = typeof(IgnoreExplicitTarget);
            _.IgnoreMember(type, "Property");
            _.IgnoreMember(type, "Field");
            _.IgnoreMember(type, "GetOnlyProperty");
            _.IgnoreMember(type, "PropertyThatThrows");
        });

        var target = new IgnoreExplicitTarget
        {
            Include = "Value",
            Field = "Value",
            Property = "Value",
            PropertyByName = "Value"
        };
        await Verifier.Verify(target, settings);

        #endregion
    }

    class IgnoreExplicitTarget
    {
        public string Include;
        public string Property { get; set; }
        public string PropertyByName { get; set; }
        public string GetOnlyProperty => "asd";
        public string PropertyThatThrows => throw new Exception();
        public string Field;
    }

    [Fact]
    public async Task CustomExceptionProp()
    {
        #region IgnoreMembersThatThrow

        var settings = new VerifySettings();
        settings.ModifySerialization(_ => _.IgnoreMembersThatThrow<CustomException>());

        var target = new WithCustomException();
        await Verifier.Verify(target, settings);

        #endregion
    }

    class WithCustomException
    {
        public Guid CustomExceptionProperty => throw new CustomException();
    }

    [Fact]
    public Task ExceptionProps()
    {
        try
        {
            throw new Exception();
        }
        catch (Exception exception)
        {
            return Verifier.Verify(exception);
        }
    }

    [Fact]
    public Task ExceptionProp()
    {
        var settings = new VerifySettings();
        settings.ModifySerialization(_ => _.IgnoreMembersThatThrow<CustomException>());

        var target = new WithException();

        return Assert.ThrowsAsync<JsonSerializationException>(() => Verifier.Verify(target, settings));
    }

    class WithException
    {
        public Guid ExceptionProperty => throw new Exception();
    }

    [Fact]
    public Task ExceptionNotIgnoreMessageProp()
    {
        var settings = new VerifySettings();
        settings.ModifySerialization(
            _ => _.IgnoreMembersThatThrow<Exception>(x => x.Message == "Ignore"));
        var target = new WithExceptionNotIgnoreMessage();

        return Assert.ThrowsAsync<JsonSerializationException>(() => Verifier.Verify(target, settings));
    }

    class WithExceptionNotIgnoreMessage
    {
        public Guid ExceptionMessageProperty => throw new Exception("NotIgnore");
    }

    [Fact]
    public Task DelegateProp()
    {
        var target = new WithDelegate();
        return Verifier.Verify(target);
    }

    class WithDelegate
    {
        public Action DelegateProperty => () => { };
    }

    class CustomException :
        Exception
    {
    }

    [Fact]
    public Task NotSupportedExceptionProp()
    {
        var target = new WithNotSupportedException();
        return Verifier.Verify(target);
    }

    class WithNotSupportedException
    {
        public Guid NotImplementedExceptionProperty => throw new NotSupportedException();
    }

    #region WithObsoletePropIncluded

    [Fact]
    public Task WithObsoletePropIncluded()
    {
        var target = new WithObsolete
        {
            ObsoleteProperty = "value1",
            OtherProperty = "value2"
        };
        var settings = new VerifySettings();
        settings.ModifySerialization(_ => { _.IncludeObsoletes(); });
        return Verifier.Verify(target, settings);
    }

    #endregion
    #region WithObsoleteProp

    class WithObsolete
    {
        [Obsolete] public string ObsoleteProperty { get; set; }
        public string OtherProperty { get; set; }
    }

    [Fact]
    public Task WithObsoleteProp()
    {
        var target = new WithObsolete
        {
            ObsoleteProperty = "value1",
            OtherProperty = "value2"
        };
        return Verifier.Verify(target);
    }

    #endregion

#pragma warning restore 612
    [Fact]
    public async Task Tuple()
    {
        var exception = await Assert.ThrowsAsync<XunitException>(() => Verifier.Verify(() => MethodWithTuple()));
        await Verifier.Verify(exception.Message);
    }

    static (bool, string, string) MethodWithTuple()
    {
        return (true, "A", "B");
    }

#endif

    void ScopedSerializer()
    {
        #region ScopedSerializer

        var person = new Person
        {
            GivenNames = "John",
            FamilyName = "Smith",
            Dob = new DateTimeOffset(2000, 10, 1, 0, 0, 0, TimeSpan.Zero),
        };
        var settings = new VerifySettings();
        settings.ModifySerialization(
            _ => _.DontScrubDateTimes());
        settings.AddExtraSettings(
            _ => { _.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat; });

        #endregion
    }
}