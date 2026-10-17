using DevTools.Testing;
using NeuralSuperchargerOwnership.Patch;

namespace NeuralSuperchargerOwnership.Tests;

// The auto-use patch targets the compiler-generated Validator local function inside
// JobGiver_GetNeuralSupercharge.ClosestSupercharger, found by name, and reads the closure's
// captured pawn through ___pawn.
[TestFixture(TestType.MainMenu)]
internal sealed class JobGiverValidatorTargetTests
{
    [Test]
    public static void FindsExactlyOneValidatorMethod()
    {
        var methods = RimWorld_JobGiver_GetNeuralSupercharge.CalculateMethods().ToList();

        Expect.AreEqual(1, methods.Count);
        if (methods.Count != 1)
        {
            return;
        }

        var method = methods[0];
        Expect.IsTrue(method.Name.Contains("Validator", StringComparison.Ordinal));
        Expect.AreEqual(typeof(JobGiver_GetNeuralSupercharge), method.DeclaringType?.DeclaringType);
    }

    [Test]
    public static void ValidatorDeclaringTypeCapturesPawn()
    {
        var method = RimWorld_JobGiver_GetNeuralSupercharge.CalculateMethods().FirstOrDefault();
        Expect.IsNotNull(method);
        if (method == null)
        {
            return;
        }

        Expect.IsFalse(method.IsStatic, "___pawn needs an instance method on the closure class");
        var pawnField = AccessTools.Field(method.DeclaringType, "pawn");
        Expect.IsNotNull(pawnField);
        Expect.AreEqual(typeof(Pawn), pawnField?.FieldType);
    }
}
