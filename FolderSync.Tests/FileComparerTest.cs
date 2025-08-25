using FolderSync.Utils;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Tests
{
    public class FileComparerTest
    {

        [Fact]
        public void Files_WithSameContent_ShouldBeEqual()
        {
            // Arrange
            var file1 = Path.GetTempFileName();
            var file2 = Path.GetTempFileName();
            File.WriteAllText(file1, "Hello world!");
            File.WriteAllText(file2, "Hello world!");

            // Act
            bool result = FileComparer.AreFilesEqual(file1, file2);

            // Assert
            Assert.True(result);

            // Cleanup
            File.Delete(file1);
            File.Delete(file2);
        }
        [Fact]
        public void GetDiffrece()
        {
            // Arrange
            var replica = Path.GetTempFileName();
            var source = Path.GetTempFileName();
            File.WriteAllLines(source, new[] { "1", "a1", "a2", "2", "3" });
            File.WriteAllLines(replica, new[] { "1", "2", "3", "u1" });
            // Act
            var differences = FileComparer.GetDifferents(source, replica);
            // Assert
            var ans = new List<(bool, string, int)>
            {
                (true, "a1", 1),
                (true, "a2", 2),
                (false, "u1", 3)
            };
            Assert.Equal(ans, differences);
            // Cleanup
            File.Delete(source);
            File.Delete(replica);
        }
        [Fact]
        public void GetDifferents_ShouldReturnEmptyList_WhenFilesAreIdentical()
        {
            // Arrange
            var file1 = Path.GetTempFileName();
            var file2 = Path.GetTempFileName();
            File.WriteAllText(file1, "Hello world!");
            File.WriteAllText(file2, "Hello world!");
            // Act
            var differences = FileComparer.GetDifferents(file1, file2);
            // Assert
            Assert.Empty(differences);
            // Cleanup
            File.Delete(file1);
            File.Delete(file2);
        }
        [Fact]
        public void GetDifferents_ShouldReducesCorrespondingElements_WhenSourceHasNewLines()
        {
            // Arrange
            var replica = Path.GetTempFileName();
            var source = Path.GetTempFileName();
            File.WriteAllLines(source, new[] { "a1", "2", });
            File.WriteAllLines(replica, new[] { "2" });
            // Act
            var differences = FileComparer.GetDifferents(source, replica);
            // Assert
            var ans = new List<(bool, string, int)>
            {
                (true, "a1", 0)
            };
            Assert.Equal(ans, differences);
            // Cleanup
            File.Delete(source);
            File.Delete(replica);
        }
        [Fact]
        public void GetDifferents_ShouldreturnChangedLines_WhenFilesHaveDifferentContent()
        {
            // Arrange
            var file1 = Path.GetTempFileName();
            var file2 = Path.GetTempFileName();
            File.WriteAllLines(file1, new[] { "Line 1", "Line 2", "Line 3" });
            File.WriteAllLines(file2, new[] { "Line 1", "Line X", "Line 3" });
            // Act
            var differences = FileComparer.GetDifferents(file1, file2);
            // Assert
            var expectedDifferences = new List<(bool, string, int)>
            {
                (true, "Line 2", 1),
                (false, "Line X", 1)
            };
            Assert.Equal(expectedDifferences, differences);
            // Cleanup
            File.Delete(file1);
            File.Delete(file2);
        }
        [Fact]
        public void GetDifferents_TwoAdjacentLinesChanged()
        {
            // Arrange
            var source = Path.GetTempFileName();
            var replica = Path.GetTempFileName();
            File.WriteAllLines(source, new[] { "1", "A", "B", "4" });
            File.WriteAllLines(replica, new[] { "1", "X", "Y", "4" });

            // Act
            var differences = FileComparer.GetDifferents(source, replica);

            // Assert
            var expected = new List<(bool, string, int)>
            {
                (true, "A", 1),
                (false, "X", 1),
                (true, "B", 2),
                (false, "Y", 2)
            };
            Assert.Equal(expected, differences);

            // Cleanup
            File.Delete(source);
            File.Delete(replica);
        }

        [Fact]
        public void GetDifferents_LineAddedInSource()
        {
            // Arrange
            var source = Path.GetTempFileName();
            var replica = Path.GetTempFileName();
            File.WriteAllLines(source, new[] { "1", "2", "NEW", "3" });
            File.WriteAllLines(replica, new[] { "1", "2", "3" });

            // Act
            var differences = FileComparer.GetDifferents(source, replica);

            // Assert
            var expected = new List<(bool, string, int)>
    {
        (true, "NEW", 2)
    };
            Assert.Equal(expected, differences);

            // Cleanup
            File.Delete(source);
            File.Delete(replica);
        }

        [Fact]
        public void GetDifferents_LineRemovedFromSource()
        {
            // Arrange
            var source = Path.GetTempFileName();
            var replica = Path.GetTempFileName();
            File.WriteAllLines(source, new[] { "1", "2", "3" });
            File.WriteAllLines(replica, new[] { "1", "2", "REMOVED", "3" });

            // Act
            var differences = FileComparer.GetDifferents(source, replica);

            // Assert
            var expected = new List<(bool, string, int)>
    {
        (false, "REMOVED", 2)
    };
            Assert.Equal(expected, differences);

            // Cleanup
            File.Delete(source);
            File.Delete(replica);
        }

        [Fact]
        public void GetDifferents_OneLineAddedOneLineRemoved()
        {
            // Arrange
            var source = Path.GetTempFileName();
            var replica = Path.GetTempFileName();
            File.WriteAllLines(source, new[] { "1", "a", "2", "3" });
            File.WriteAllLines(replica, new[] { "1", "2", "3", "u" });

            // Act
            var differences = FileComparer.GetDifferents(source, replica);

            // Assert
            var expected = new List<(bool, string, int)>
    {
        (true, "a", 1),
        (false, "u", 3)
    };
            Assert.Equal(expected, differences);

            // Cleanup
            File.Delete(source);
            File.Delete(replica);
        }

        // Dodatkowe testy

        [Fact]
        public void GetDifferents_SourceIsEmpty_ReplicaHasContent()
        {
            // Arrange
            var source = Path.GetTempFileName();
            var replica = Path.GetTempFileName();
            File.WriteAllLines(source, Array.Empty<string>());
            File.WriteAllLines(replica, new[] { "A", "B" });

            // Act
            var differences = FileComparer.GetDifferents(source, replica);

            // Assert
            var expected = new List<(bool, string, int)>
    {
        (false, "A", 0),
        (false, "B", 1)
    };
            Assert.Equal(expected, differences);

            // Cleanup
            File.Delete(source);
            File.Delete(replica);
        }

        [Fact]
        public void GetDifferents_ReplicaIsEmpty_SourceHasContent()
        {
            // Arrange
            var source = Path.GetTempFileName();
            var replica = Path.GetTempFileName();
            File.WriteAllLines(source, new[] { "A", "B" });
            File.WriteAllLines(replica, Array.Empty<string>());

            // Act
            var differences = FileComparer.GetDifferents(source, replica);

            // Assert
            var expected = new List<(bool, string, int)>
    {
        (true, "A", 0),
        (true, "B", 1)
    };
            Assert.Equal(expected, differences);

            // Cleanup
            File.Delete(source);
            File.Delete(replica);
        }

        [Fact]
        public void GetDifferents_MultipleNonAdjacentChanges()
        {
            // Arrange
            var source = Path.GetTempFileName();
            var replica = Path.GetTempFileName();
            File.WriteAllLines(source, new[] { "1", "A", "2", "B", "3", "C" });
            File.WriteAllLines(replica, new[] { "1", "X", "2", "Y", "3" });

            // Act
            var differences = FileComparer.GetDifferents(source, replica);

            // Assert
            var expected = new List<(bool, string, int)>
    {
        (true, "A", 1),
        (false, "X", 1),
        (true, "B", 3),
        (false, "Y", 3),
        (true, "C", 5)
    };
            Assert.Equal(expected, differences);

            // Cleanup
            File.Delete(source);
            File.Delete(replica);
        }



    }
}
