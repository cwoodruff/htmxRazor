using htmxRazor.Components.Formatting;
using Xunit;

namespace htmxRazor.Tests;

public class RelativeTimeTests : TagHelperTestBase
{
    private static readonly DateTimeOffset Now = new(2025, 3, 15, 12, 0, 0, TimeSpan.Zero);

    // ══════════════════════════════════════════════
    //  RelativeTimeFormatter — Long format
    // ══════════════════════════════════════════════

    [Fact]
    public void Just_Now()
    {
        var date = Now.AddSeconds(-3);
        Assert.Equal("just now", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void Seconds_Ago()
    {
        var date = Now.AddSeconds(-30);
        Assert.Equal("30 seconds ago", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void One_Minute_Ago()
    {
        var date = Now.AddMinutes(-1);
        Assert.Equal("1 minute ago", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void Minutes_Ago()
    {
        var date = Now.AddMinutes(-5);
        Assert.Equal("5 minutes ago", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void One_Hour_Ago()
    {
        var date = Now.AddHours(-1);
        Assert.Equal("1 hour ago", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void Hours_Ago()
    {
        var date = Now.AddHours(-3);
        Assert.Equal("3 hours ago", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void One_Day_Ago()
    {
        var date = Now.AddDays(-1);
        Assert.Equal("1 day ago", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void Days_Ago()
    {
        var date = Now.AddDays(-5);
        Assert.Equal("5 days ago", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void Weeks_Ago()
    {
        var date = Now.AddDays(-14);
        Assert.Equal("2 weeks ago", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void Months_Ago()
    {
        var date = Now.AddDays(-60);
        Assert.Equal("2 months ago", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void Years_Ago()
    {
        var date = Now.AddDays(-400);
        Assert.Equal("1 year ago", RelativeTimeFormatter.Format(date, Now));
    }

    // ══════════════════════════════════════════════
    //  Future
    // ══════════════════════════════════════════════

    [Fact]
    public void In_A_Moment()
    {
        var date = Now.AddSeconds(3);
        Assert.Equal("in a moment", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void In_Minutes()
    {
        var date = Now.AddMinutes(10);
        Assert.Equal("in 10 minutes", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void In_Hours()
    {
        var date = Now.AddHours(2);
        Assert.Equal("in 2 hours", RelativeTimeFormatter.Format(date, Now));
    }

    [Fact]
    public void In_Days()
    {
        var date = Now.AddDays(3);
        Assert.Equal("in 3 days", RelativeTimeFormatter.Format(date, Now));
    }

    // ══════════════════════════════════════════════
    //  Auto numeric
    // ══════════════════════════════════════════════

    [Fact]
    public void Auto_Yesterday()
    {
        var date = Now.AddDays(-1);
        Assert.Equal("yesterday", RelativeTimeFormatter.Format(date, Now, "long", "auto"));
    }

    [Fact]
    public void Auto_Tomorrow()
    {
        var date = Now.AddDays(1);
        Assert.Equal("tomorrow", RelativeTimeFormatter.Format(date, Now, "long", "auto"));
    }

    [Fact]
    public void Auto_Last_Week()
    {
        var date = Now.AddDays(-7);
        Assert.Equal("last week", RelativeTimeFormatter.Format(date, Now, "long", "auto"));
    }

    [Fact]
    public void Auto_Next_Month()
    {
        var date = Now.AddDays(30);
        Assert.Equal("next month", RelativeTimeFormatter.Format(date, Now, "long", "auto"));
    }

    [Fact]
    public void Auto_Last_Year()
    {
        var date = Now.AddDays(-365);
        Assert.Equal("last year", RelativeTimeFormatter.Format(date, Now, "long", "auto"));
    }

    [Fact]
    public void Auto_Multiple_Days_Not_Auto()
    {
        // "auto" only applies when value == 1
        var date = Now.AddDays(-3);
        Assert.Equal("3 days ago", RelativeTimeFormatter.Format(date, Now, "long", "auto"));
    }

    // ══════════════════════════════════════════════
    //  Short format
    // ══════════════════════════════════════════════

    [Fact]
    public void Short_Now()
    {
        var date = Now.AddSeconds(-3);
        Assert.Equal("now", RelativeTimeFormatter.Format(date, Now, "short"));
    }

    [Fact]
    public void Short_Minutes_Ago()
    {
        var date = Now.AddMinutes(-5);
        Assert.Equal("5m ago", RelativeTimeFormatter.Format(date, Now, "short"));
    }

    [Fact]
    public void Short_Hours_Future()
    {
        var date = Now.AddHours(2);
        Assert.Equal("in 2h", RelativeTimeFormatter.Format(date, Now, "short"));
    }

    [Fact]
    public void Short_Days_Ago()
    {
        var date = Now.AddDays(-3);
        Assert.Equal("3d ago", RelativeTimeFormatter.Format(date, Now, "short"));
    }

    // ══════════════════════════════════════════════
    //  Narrow format
    // ══════════════════════════════════════════════

    [Fact]
    public void Narrow_Now()
    {
        var date = Now.AddSeconds(-3);
        Assert.Equal("now", RelativeTimeFormatter.Format(date, Now, "narrow"));
    }

    [Fact]
    public void Narrow_Minutes()
    {
        var date = Now.AddMinutes(-5);
        Assert.Equal("5m", RelativeTimeFormatter.Format(date, Now, "narrow"));
    }

    [Fact]
    public void Narrow_Hours()
    {
        var date = Now.AddHours(2);
        Assert.Equal("2h", RelativeTimeFormatter.Format(date, Now, "narrow"));
    }

    [Fact]
    public void Narrow_Months()
    {
        var date = Now.AddDays(-45);
        Assert.Equal("1mo", RelativeTimeFormatter.Format(date, Now, "narrow"));
    }

    // ══════════════════════════════════════════════
    //  RelativeTimeTagHelper — Structure
    // ══════════════════════════════════════════════

    [Fact]
    public void TagHelper_Renders_Time_Element()
    {
        var helper = new RelativeTimeTagHelper { Date = Now.AddMinutes(-5) };
        var context = CreateContext("rhx-relative-time");
        var output = CreateOutput("rhx-relative-time");

        helper.Process(context, output);

        Assert.Equal("time", output.TagName);
    }

    [Fact]
    public void TagHelper_Has_Block_Class()
    {
        var helper = new RelativeTimeTagHelper { Date = Now.AddMinutes(-5) };
        var context = CreateContext("rhx-relative-time");
        var output = CreateOutput("rhx-relative-time");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-relative-time"));
    }

    [Fact]
    public void TagHelper_Has_Datetime_Attribute()
    {
        var helper = new RelativeTimeTagHelper { Date = Now };
        var context = CreateContext("rhx-relative-time");
        var output = CreateOutput("rhx-relative-time");

        helper.Process(context, output);

        Assert.NotNull(GetAttribute(output, "datetime"));
    }

    [Fact]
    public void TagHelper_Renders_Text_Server_Side_Without_Js_Hooks()
    {
        var helper = new RelativeTimeTagHelper { Date = Now.AddHours(-2) };
        var context = CreateContext("rhx-relative-time");
        var output = CreateOutput("rhx-relative-time");

        helper.Process(context, output);

        // Text is rendered on the server; no JS-only data hooks remain.
        Assert.False(string.IsNullOrWhiteSpace(output.Content.GetContent()));
        Assert.NotNull(GetAttribute(output, "datetime"));
        AssertNoAttribute(output, "data-rhx-relative-time");
        AssertNoAttribute(output, "data-rhx-relative-format");
        AssertNoAttribute(output, "data-rhx-relative-numeric");
    }

    [Fact]
    public void TagHelper_Format_Affects_Rendered_Text()
    {
        var longHelper = new RelativeTimeTagHelper { Date = Now.AddHours(-2), Format = "long" };
        var shortHelper = new RelativeTimeTagHelper { Date = Now.AddHours(-2), Format = "short" };

        var longOut = CreateOutput("rhx-relative-time");
        var shortOut = CreateOutput("rhx-relative-time");
        longHelper.Process(CreateContext("rhx-relative-time"), longOut);
        shortHelper.Process(CreateContext("rhx-relative-time"), shortOut);

        // Different formats produce different server-rendered text.
        Assert.NotEqual(longOut.Content.GetContent(), shortOut.Content.GetContent());
    }
}
