using CivicFlow.Domain.Entities;
using FluentAssertions;

namespace CivicFlow.UnitTests.Domain;

public class ReviewCommentSoftDeleteTests
{
    [Fact]
    public void NewReviewComment_IsNotDeleted()
    {
        var comment = new ReviewComment { Content = "Test", AuthorId = "user-1", PermitApplicationId = 1 };
        comment.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void SoftDeleting_ReviewComment_SetsIsDeletedTrue()
    {
        var comment = new ReviewComment { Content = "Test", AuthorId = "user-1", PermitApplicationId = 1 };
        comment.IsDeleted = true;
        comment.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void InternalComment_IsNotVisibleToApplicants()
    {
        var internalComment = new ReviewComment { Content = "Internal note", IsInternal = true, AuthorId = "staff-1", PermitApplicationId = 1 };
        var applicantVisibleComments = new List<ReviewComment> { internalComment }
            .Where(c => !c.IsInternal && !c.IsDeleted)
            .ToList();

        applicantVisibleComments.Should().BeEmpty();
    }

    [Fact]
    public void PublicComment_IsVisibleToApplicants()
    {
        var publicComment = new ReviewComment { Content = "Please update Section 4", IsInternal = false, AuthorId = "staff-1", PermitApplicationId = 1 };
        var applicantVisibleComments = new List<ReviewComment> { publicComment }
            .Where(c => !c.IsInternal && !c.IsDeleted)
            .ToList();

        applicantVisibleComments.Should().ContainSingle();
    }

    [Fact]
    public void DeletedPublicComment_IsNotVisibleToApplicants()
    {
        var deletedComment = new ReviewComment { Content = "Retracted", IsInternal = false, IsDeleted = true, AuthorId = "staff-1", PermitApplicationId = 1 };
        var applicantVisibleComments = new List<ReviewComment> { deletedComment }
            .Where(c => !c.IsInternal && !c.IsDeleted)
            .ToList();

        applicantVisibleComments.Should().BeEmpty();
    }

    [Fact]
    public void MixedComments_FilterReturnsOnlyPublicNonDeleted()
    {
        var comments = new List<ReviewComment>
        {
            new() { Content = "Public visible",   IsInternal = false, IsDeleted = false, AuthorId = "staff", PermitApplicationId = 1 },
            new() { Content = "Internal note",     IsInternal = true,  IsDeleted = false, AuthorId = "staff", PermitApplicationId = 1 },
            new() { Content = "Deleted public",    IsInternal = false, IsDeleted = true,  AuthorId = "staff", PermitApplicationId = 1 },
            new() { Content = "Deleted internal",  IsInternal = true,  IsDeleted = true,  AuthorId = "staff", PermitApplicationId = 1 },
        };

        var visible = comments.Where(c => !c.IsInternal && !c.IsDeleted).ToList();
        visible.Should().HaveCount(1).And.Contain(c => c.Content == "Public visible");
    }
}
