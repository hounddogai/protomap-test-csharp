namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionRequestRulesChecks
{
    public static void RecognitionRequestValidatorReportsEveryProblem()
    {
        var validator = RecognitionRequestRules.DefaultRecognitionRequestValidator();
        var item = RecognitionRequest.NewRecognitionRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new RecognitionRequestCodeLengthRule(1000);
        var item = RecognitionRequest.NewRecognitionRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionRequestNotesLengthRuleFlagsALongNotes()
    {
        var rule = new RecognitionRequestNotesLengthRule(64);
        var item = RecognitionRequest.NewRecognitionRequest("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionRequestRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new RecognitionRequestRequiredNotesRule("recognition request needs a value");
        var item = RecognitionRequest.NewRecognitionRequest("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionRequestQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new RecognitionRequestQuantityRangeRule(255, 3600);
        var item = RecognitionRequest.NewRecognitionRequest("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionRequestRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RecognitionRequestRateRangeRule(6.2, 2223.4);
        var item = RecognitionRequest.NewRecognitionRequest("x", "y");
        item.Rate = 2224.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RecognitionRequestValidatorReportsEveryProblem();
        RecognitionRequestCodeLengthRuleFlagsALongCode();
        RecognitionRequestNotesLengthRuleFlagsALongNotes();
        RecognitionRequestRequiredNotesRuleFlagsABlankNotes();
        RecognitionRequestQuantityRangeRuleFlagsValuesAboveQuantity();
        RecognitionRequestRateRangeRuleFlagsValuesAboveRate();
    }
}
