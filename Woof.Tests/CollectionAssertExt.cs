public class CollectionAssertExt
{
    public static void AreEqual<T>(IList<T> expected, IList<T> actual)
    {
        if (expected.Count != actual.Count)
        {
            var actualStr = string.Join(" , ", actual.Select(x => x?.ToString() ?? "null"));
            throw new AssertFailedException($"expected {expected.Count} elements but had {actual.Count}: {actualStr}");
        }

        for (int i = 0; i < expected.Count; i++)
        {
            Assert.AreEqual(expected[i], actual[i]);
        }
    }
}
