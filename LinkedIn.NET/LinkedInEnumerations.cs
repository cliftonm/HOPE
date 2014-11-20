// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;

namespace LinkedIn.NET
{
    /// <summary>
    /// Specifies identifiers to indicate LinkedIn permissions scope while requesting for access token
    /// </summary>
    [Flags]
    public enum LinkedInPermissions
    {
        /// <summary>
        /// Profile overvew - name, photo, headline, and current positions
        /// </summary>
        BasicProfile = 1,
        /// <summary>
        /// Full profile including experience, education, skills, and recommendations
        /// </summary>
        FullProfile = 2,
        /// <summary>
        /// The primary email address user uses for his LinkedIn account
        /// </summary>
        EmailAddress = 4,
        /// <summary>
        /// 1st and 2nd degree connections
        /// </summary>
        Connections = 8,
        /// <summary>
        /// Contacts info - address, phone number, and bound accounts
        /// </summary>
        ContactsInfo = 16,
        /// <summary>
        /// Retrieves and posts updates to LinkedIn as user
        /// </summary>
        Updates = 32,
        /// <summary>
        /// Retrieves and posts group discussions as user
        /// </summary>
        GroupDiscussions = 64,
        /// <summary>
        /// Sends messages and invitations to connect as user
        /// </summary>
        Messages = 128,
        /// <summary>
        /// Full scope of permissions
        /// </summary>
        FullPermissionsScope = FullProfile | EmailAddress | Connections | ContactsInfo | Updates | GroupDiscussions | Messages
    }

    /// <summary>
    /// Specifies identifiers to indicate which LinkedIn updates should be retrieved
    /// </summary>
    [Flags]
    public enum LinkedInUpdateType
    {
        /// <summary>
        /// Nothing
        /// </summary>
        None = 0,
        /// <summary>
        /// An action that occurred in a partner application either by a connection or by an application itself
        /// </summary>
        ApplicationUpdate = 1,
        /// <summary>
        /// A change to one of the companies the member is following
        /// </summary>
        CompanyFollowUpdate = 2,
        /// <summary>
        /// These updates cover aspects of connections made on LinkedIn. They cover both the user connecting and the user's connections making connections (second degree connections)
        /// </summary>
        ConnectionUpdate = 4,
        /// <summary>
        /// A connection has posted a job posting on LinkedIn
        /// </summary>
        PostedJobUpdate = 8,
        /// <summary>
        /// A connection has joined a group
        /// </summary>
        JoinedGroupUpdate = 16,
        /// <summary>
        /// A connection has updated their profile picture
        /// </summary>
        ChangedPictureUpdate = 32,
        /// <summary>
        /// A connection has updated their extended profile, personal information such as phone number, IM account, and Twitter handle
        /// </summary>
        ExtendedProfileUpdate = 64,
        /// <summary>
        /// A connection was recommended
        /// </summary>
        RecommendationUpdate = 128,
        /// <summary>
        /// A connection has updated their profile. This does not include picture updates, which are covered under PICT type
        /// </summary>
        ChangedProfileUpdate = 256,
        /// <summary>
        /// A connection has shared an update or link
        /// </summary>
        SharedItemUpdate = 512,
        /// <summary>
        /// A connection has updated their status. This update type is deprecated in favor of SHAR
        /// </summary>
        StatusUpdate = 1024,
        /// <summary>
        /// A connection has commented on or liked another update
        /// </summary>
        ViralUpdate = 2048,
        /// <summary>
        /// All available update types
        /// </summary>
        AllAvailableUpdateTypes = ApplicationUpdate | CompanyFollowUpdate | ConnectionUpdate | PostedJobUpdate | JoinedGroupUpdate | ChangedPictureUpdate | ExtendedProfileUpdate | RecommendationUpdate | ChangedProfileUpdate | SharedItemUpdate | StatusUpdate | ViralUpdate
    }

    /// <summary>
    /// Specifies identifiers to indicate which scope is used while retrieving LinkedIn updates
    /// </summary>
    public enum LinkedInUpdateScope
    {
        /// <summary>
        /// To return aggregated network feed
        /// </summary>
        AggregatedFeed,
        /// <summary>
        /// To return member's feed
        /// </summary>
        Self,
        /// <summary>
        /// To return all feed
        /// </summary>
        All
    }

    /// <summary>
    /// Specifies identifiers to indicate which <see cref="LinkedIn.NET.Updates.LinkedInUpdate"/> field will be used as sort field while retrieving LinkedIn updates
    /// </summary>
    public enum LinkedInUpdateSortField
    {
        /// <summary>
        /// Udpdate date (see <see cref="LinkedIn.NET.Updates.LinkedInUpdate.UpdateDate"/>)
        /// </summary>
        UpdateDate,
        /// <summary>
        /// Update key (see <see cref="LinkedIn.NET.Updates.LinkedInUpdate.UpdateKey"/>)
        /// </summary>
        UpdateKey,
        /// <summary>
        /// Update type (see <see cref="LinkedIn.NET.Updates.LinkedInUpdate.UpdateType"/>)
        /// </summary>
        UpdateType
    }

    /// <summary>
    /// Specifies identifiers to indicate how LinkedIn updates will be sorted after retrieving
    /// </summary>
    public enum LinkedInUpdateSortDirection
    {
        /// <summary>
        /// Sort ascending
        /// </summary>
        Ascending,
        /// <summary>
        /// Sort descending
        /// </summary>
        Descending
    }

    /// <summary>
    /// Specifies identifiers to indicate what is visibility scope of LinkedIn share
    /// </summary>
    public enum LinkedInShareVisibilityCode
    {
        /// <summary>
        /// Any LinkedIn member
        /// </summary>
        Anyone,
        /// <summary>
        /// Currently logged in user's connections only
        /// </summary>
        ConnectionsOnly
    }

    /// <summary>
    /// Specifies identifiers to indicate LinkedIn patent status
    /// </summary>
    public enum LinkedInPatentStatusType
    {
        /// <summary>
        /// Patent has been applicated
        /// </summary>
        Application,
        /// <summary>
        /// Granted patent
        /// </summary>
        Granted
    }

    /// <summary>
    /// Specifies identifiers to indicate LinkedIn recommendation type
    /// </summary>
    public enum LinkedInRecommendationType
    {
        /// <summary>
        /// Given recommendation
        /// </summary>
        Given,
        /// <summary>
        /// Received recommendation
        /// </summary>
        Received
    }

    /// <summary>
    /// Specifies identifiers to indicate which fields of basic profile should be retrieved
    /// </summary>
    public enum LinkedInBasicProfileFields
    {
        /// <summary>
        /// A unique identifier token for this member
        /// </summary>
        Id,
        /// <summary>
        /// The member's first name
        /// </summary>
        FirstName,
        /// <summary>
        /// The member's last name
        /// </summary>
        LastName,
        /// <summary>
        /// The member's maiden name
        /// </summary>
        MaidenName,
        /// <summary>
        /// The member's name formatted based on language
        /// </summary>
        FormattedName,
        /// <summary>
        /// The member's first name spelled phonetically
        /// </summary>
        PhoneticFirstName,
        /// <summary>
        /// The member's last name spelled phonetically
        /// </summary>
        PhoneticLastName,
        /// <summary>
        /// The member's name spelled phonetically and formatted based on language
        /// </summary>
        FormattedPhoneticName,
        /// <summary>
        /// The member's headline
        /// </summary>
        Headline,
        /// <summary>
        /// The member's location
        /// </summary>
        Location,
        /// <summary>
        /// The industry the LinkedIn member has indicated their profile belongs to
        /// </summary>
        Industry,
        /// <summary>
        /// The degree distance of the fetched profile from the member who fetched the profile
        /// </summary>
        Distance,
        /// <summary>
        /// The degree distance of the fetched profile from the member who fetched the profile
        /// </summary>
        RelationToViewer,
        /// <summary>
        /// The member's current share, if set
        /// </summary>
        CurrentShare,
        /// <summary>
        /// The number of connections the member has
        /// </summary>
        NumConnections,
        /// <summary>
        /// The parameter indicating whether value of num-connections has been capped at 500
        /// </summary>
        NumConnectionsCapped,
        /// <summary>
        /// The description of member's professional profile
        /// </summary>
        Summary,
        /// <summary>
        /// The description of member's specialties
        /// </summary>
        Specialities,
        /// <summary>
        /// A collection of positions a member has had
        /// </summary>
        Positions,
        /// <summary>
        /// A URL to the profile picture, if the member has associated one with their profile and it is visible to the requestor
        /// </summary>
        PictureUrl,
        /// <summary>
        /// The URL to the member's authenticated profile on LinkedIn
        /// </summary>
        SiteStandardProfileRequest,
        /// <summary>
        /// A URL representing the resource you would request for programmatic access to the member's profile and collection of fields that can be re-used as HTTP headers to request an out of network profile programmatically
        /// </summary>
        ApiStandardProfileRequest,
        /// <summary>
        /// A URL to the member's public profile, if enabled
        /// </summary>
        PublicProfileUrl
    }

    /// <summary>
    /// Specifies identifiers to indicate which fields of email profile should be retrieved
    /// </summary>
    public enum LinkedInEmailProfileFields
    {
        /// <summary>
        /// The primary email address of user
        /// </summary>
        EmailAddress
    }

    /// <summary>
    /// Specifies identifiers to indicate which fields of full profile should be retrieved
    /// </summary>
    public enum LinkedInFullProfileFields
    {
        /// <summary>
        /// The timestamp when the member's profile was last edited
        /// </summary>
        LastModifiedTimestamp,
        /// <summary>
        /// The text describing how the member approaches proposals
        /// </summary>
        ProposalComments,
        /// <summary>
        /// The text enumerating the Associations a member has
        /// </summary>
        Associations,
        /// <summary>
        /// The text describing the member's interests
        /// </summary>
        Interests,
        /// <summary>
        /// A collection of publications authored by this member
        /// </summary>
        Publications,
        /// <summary>
        /// A collection of patents or patent applications held by this member
        /// </summary>
        Patents,
        /// <summary>
        /// A collection of languages and the level of the member's proficiency for each
        /// </summary>
        Languages,
        /// <summary>
        /// A collection of skills held by this member
        /// </summary>
        Skills,
        /// <summary>
        /// A collection of certifications earned by this member
        /// </summary>
        Certifications,
        /// <summary>
        /// A collection of education institutions a member has attended
        /// </summary>
        Educations,
        /// <summary>
        /// A collection of courses a member has taken
        /// </summary>
        Courses,
        /// <summary>
        /// A collection of volunteering experiences a member has participated in, including organizations and causes
        /// </summary>
        Volunteer,
        /// <summary>
        /// A collection of positions a member currently holds, limited to three
        /// </summary>
        ThreeCurrentPositions,
        /// <summary>
        /// A collection of positions a member formerly held, limited to the three most recent
        /// </summary>
        ThreeLastPositions,
        /// <summary>
        /// The number of recommendations the member has
        /// </summary>
        NumRecommenders,
        /// <summary>
        /// A collection of recommendations a member has received
        /// </summary>
        RecommendationsReceived,
        /// <summary>
        /// A URL for the member's multiple feeds
        /// </summary>
        MfeedRssUrl,
        /// <summary>
        /// A collection of people, company, and industries that the member is following
        /// </summary>
        Following,
        /// <summary>
        /// A collection of jobs that the member is following
        /// </summary>
        JobBookmarks,
        /// <summary>
        /// A collection of people, company, and industries suggested for the member to follow
        /// </summary>
        Suggestions,
        /// <summary>
        /// Member's birth date
        /// </summary>
        DateOfBirth,
        /// <summary>
        /// A collection of URLs the member has chosen to share on their LinkedIn profile
        /// </summary>
        MemberUrlResources,
        /// <summary>
        /// A collection of related profiles that were viewed before or after the member's profile
        /// </summary>
        RelatedProfileViews
    }

    /// <summary>
    /// Specifies identifiers to indicate the source for each member in multiple members request
    /// </summary>
    public enum LinkedInGetMemberBy
    {
        /// <summary>
        /// Currently logged user
        /// </summary>
        Self,
        /// <summary>
        /// Member's Id
        /// </summary>
        Id,
        /// <summary>
        /// Member's public URL
        /// </summary>
        Url
    }

    /// <summary>
    /// Specifies identifiers to indicate how search results should be ordered
    /// </summary>
    public enum LinkedinSearchResultsOrder
    {
        /// <summary>
        /// Sort by number of connections per person, from largest to smallest.
        /// </summary>
        Connections,
        /// <summary>
        /// Sort by number of recommendations per person, from largest to smallest.
        /// </summary>
        Recommenders,
        /// <summary>
        /// Sort by degree of separation within the member's network
        /// </summary>
        Distance,
        /// <summary>
        /// Sort by relevance of results based on the query, from most to least relevant.
        /// </summary>
        Relevance
    }

    /// <summary>
    /// Specifies identifiers to indicate what sets of data should be included in search results
    /// </summary>
    [Flags]
    public enum LinkedInSearchSets
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// People set
        /// </summary>
        People = 1,
        /// <summary>
        /// Facets set
        /// </summary>
        Facets = 2,
    }

    /// <summary>
    /// Specifies identifiers to indicate which fields of facet should be included in search results
    /// </summary>
    public enum LinkedInFacetFields
    {
        /// <summary>
        /// A human readable name for the facet
        /// </summary>
        Name,
        /// <summary>
        /// The machine processable value for the facet
        /// </summary>
        Code
    }

    /// <summary>
    /// Specifies identifiers to indicate which fields of buckets should be included in search results
    /// </summary>
    public enum LinkedInBucketFields
    {
        /// <summary>
        /// A human readable name for the facet bucket
        /// </summary>
        Name,
        /// <summary>
        /// The machine processable value for the bucket
        /// </summary>
        Code,
        /// <summary>
        /// The number of results inside the bucket
        /// </summary>
        Count,
        /// <summary>
        /// Specifies whether this bucket's results are included in your search query
        /// </summary>
        Selected
    }

    /// <summary>
    /// Specifies identifiers to indicate which types of facets should be included in query
    /// </summary>
    [Flags]
    public enum LinkedInFacetTypes
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// A geographical region.
        /// </summary>
        Location = 1,
        /// <summary>
        /// An industry field.
        /// </summary>
        Industry = 2,
        /// <summary>
        /// A specific relationship to the member's social network.
        /// </summary>
        Network = 4,
        /// <summary>
        /// A member locale set to a specific language.
        /// </summary>
        Language = 8,
        /// <summary>
        /// A member's current companies.
        /// </summary>
        CurrentCompany = 16,
        /// <summary>
        /// A member's past companies.
        /// </summary>
        PastCompany = 32,
        /// <summary>
        /// A members current or previous school.
        /// </summary>
        School = 64
    }

    /// <summary>
    /// Specifies identifiers to indicate which types of network facet should be included in query
    /// </summary>
    [Flags]
    public enum LinkedInFacetNetwork
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// First degree connections
        /// </summary>
        FirstDegree = 1,
        /// <summary>
        /// Second degree connections
        /// </summary>
        SecondDegree = 2,
        /// <summary>
        /// Inside one of user's groups
        /// </summary>
        InsideGroup = 4,
        /// <summary>
        /// Out-of-network connections
        /// </summary>
        OutOfNetwork = 8
    }

    /// <summary>
    /// Specifies identifiers to indicate which types of language facet should be included in query
    /// </summary>
    public enum LinkedInFacetLanguage
    {
        /// <summary>
        /// English
        /// </summary>
        English,
        /// <summary>
        /// Russian
        /// </summary>
        Russian,
        /// <summary>
        /// Spanish
        /// </summary>
        Spanish,
        /// <summary>
        /// French
        /// </summary>
        French,
        /// <summary>
        /// German
        /// </summary>
        German,
        /// <summary>
        /// Italian
        /// </summary>
        Italian,
        /// <summary>
        /// Portuguese
        /// </summary>
        Portuguese,
        /// <summary>
        /// Other languages
        /// </summary>
        Others
    }

    /// <summary>
    /// Specifies identifiers to indicate current response status
    /// </summary>
    public enum LinkedInResponseStatus
    {
        /// <summary>
        /// Operation completed successfully
        /// </summary>
        OK,
        /// <summary>
        /// The request was invalid, which is usually caused by incorrect formating
        /// </summary>
        BadRequest,
        /// <summary>
        /// The access token has expired
        /// </summary>
        ExpiredToken,
        /// <summary>
        /// The access token is invalid
        /// </summary>
        InvalidAccessToken,
        /// <summary>
        /// The request has to use "https:" protocol
        /// </summary>
        SslRequired,
        /// <summary>
        /// The attempt to post share update failed
        /// </summary>
        UpdateFailed,
        /// <summary>
        /// The application tries to perform unauthorized action, e.g. categorize the post of group that member is not a member of
        /// </summary>
        UnauthorizedAction,
        /// <summary>
        /// The application has reached the throttle limit for a particular resource
        /// </summary>
        TrottleLimitReached,
        /// <summary>
        /// The endpoint or resource the application is trying to reach doesn't exist
        /// </summary>
        PageNotFound,
        /// <summary>
        /// There was an application error on the LinkedIn server. Usually the request is valid but needs to be made at a later time
        /// </summary>
        InternalServerError,
        /// <summary>
        /// An unparsed web exception
        /// </summary>
        UnparsedWebException,
        /// <summary>
        /// Other exception
        /// </summary>
        OtherException
    }

    /// <summary>
    /// Specifies identifiers to indicate LinkedIn group post type
    /// </summary>
    public enum LinkedInGroupPostType
    {
        /// <summary>
        /// Post status is not defined
        /// </summary>
        NotDefined,
        /// <summary>
        /// Standard type
        /// </summary>
        Standard,
        /// <summary>
        /// News type
        /// </summary>
        News
    }

    /// <summary>
    /// Specifies identifiers to indicate LinkedIn group post category
    /// </summary>
    public enum LinkedInGroupPostCategory
    {
        /// <summary>
        /// Post category is not defined
        /// </summary>
        NotDefined,
        /// <summary>
        /// Discussion category
        /// </summary>
        Discussion
    }

    /// <summary>
    /// Specifies identifiers to indicate LinkedIn group post comment available action
    /// </summary>
    public enum LinkedInGroupCommentAction
    {
        /// <summary>
        /// Flag comment as inappropriate
        /// </summary>
        FlagAsInappropriate,
        /// <summary>
        /// Delete comment
        /// </summary>
        Delete
    }

    /// <summary>
    /// Specifies identifiers to indicate LinkedIn group post available action
    /// </summary>
    public enum LinkedInGroupPostAction
    {
        /// <summary>
        /// Add comment to post
        /// </summary>
        AddComment,
        /// <summary>
        /// Flag post as inappropriate
        /// </summary>
        FlagAsInappropriate,
        /// <summary>
        /// Categorize post as job
        /// </summary>
        CategorizeAsJob,
        /// <summary>
        /// Categorize post as promotion
        /// </summary>
        CategorizeAsPromotion,
        /// <summary>
        /// Follow post
        /// </summary>
        Follow,
        /// <summary>
        /// Like post
        /// </summary>
        Like,
        /// <summary>
        /// Reply privately
        /// </summary>
        ReplyPrivately
    }

    /// <summary>
    /// Specifies identifiers to indicate LinkedIn group available action
    /// </summary>
    public enum LinkedInGroupAction
    {
        /// <summary>
        /// Add post to group
        /// </summary>
        AddPost,
        /// <summary>
        /// Leave group
        /// </summary>
        Leave,
        /// <summary>
        /// View group's posts
        /// </summary>
        ViewPosts
    }

    /// <summary>
    /// Specifies identifiers to indicate LinkedIn group category
    /// </summary>
    public enum LinkedInGroupCategory
    {
        /// <summary>
        /// Category is not defined
        /// </summary>
        NotDefined,
        /// <summary>
        /// Alumni group
        /// </summary>
        Alumni,
        /// <summary>
        /// Corporate group
        /// </summary>
        Corporate,
        /// <summary>
        /// Conference group
        /// </summary>
        Conference,
        /// <summary>
        /// Network group
        /// </summary>
        Network,
        /// <summary>
        /// Philantropic  group
        /// </summary>
        Philantropic,
        /// <summary>
        /// Professional group
        /// </summary>
        Professional,
        /// <summary>
        /// Other group
        /// </summary>
        Other
    }

    /// <summary>
    /// Specifies identifiers to indicate LinkedIn group relationship to the user
    /// </summary>
    public enum LinkedInGroupRelationship
    {
        /// <summary>
        /// User is blocked
        /// </summary>
        NonMember,
        /// <summary>
        /// Awaiting confirmation
        /// </summary>
        Blocked,
        /// <summary>
        /// User is not a member of group
        /// </summary>
        AwaitingConfirmation,
        /// <summary>
        /// Awaiting parent group confirmation
        /// </summary>
        AwaitingParentGroupConfirmation,
        /// <summary>
        /// User is a member of group
        /// </summary>
        Member,
        /// <summary>
        /// User is a moderator of group
        /// </summary>
        Moderator,
        /// <summary>
        /// User is a manager of group
        /// </summary>
        Manager,
        /// <summary>
        /// User is an owner of group
        /// </summary>
        Owner
    }

    /// <summary>
    /// Specifies identifiers to indicate which fields of group should be retrieved
    /// </summary>
    public enum LinkedInGroupFields
    {
        /// <summary>
        /// The group's ID
        /// </summary>
        Id,
        /// <summary>
        /// The name of the group
        /// </summary>
        Name,
        /// <summary>
        /// A short description for the group
        /// </summary>
        ShortDescription,
        /// <summary>
        /// A full description for the group
        /// </summary>
        Description,
        /// <summary>
        /// The group's relationship to the user and available actions
        /// </summary>
        RelationToViewer,
        /// <summary>
        /// The number of posts created in the past day for each category
        /// </summary>
        CountsByCategory,
        /// <summary>
        /// The value specified whether the group discussions are open to non-members
        /// </summary>
        IsOpenToNonMembers,
        /// <summary>
        /// The group's category
        /// </summary>
        Category,
        /// <summary>
        /// The external website for the group
        /// </summary>
        WebSiteUrl,
        /// <summary>
        /// The LinkedIn site URL for the group
        /// </summary>
        SiteGroupUrl,
        /// <summary>
        /// The language locale of the group
        /// </summary>
        Locale,
        /// <summary>
        /// The group's country and postal code
        /// </summary>
        Location,
        /// <summary>
        /// The value specified whether members are allowed to invite other members to join
        /// </summary>
        AllowMembersInvite,
        /// <summary>
        /// The small logo for the group, to be used when representing the group on other sites
        /// </summary>
        SmallLogoUrl,
        /// <summary>
        /// The large logo for the group, to be used when representing the group on other sites
        /// </summary>
        LargeLogoUrl,
        /// <summary>
        /// The number of members of the group
        /// </summary>
        NumberOfMembers
    }

    /// <summary>
    /// Specifies identifiers to indicate which fields of group post should be retrieved
    /// </summary>
    public enum LinkedInGroupPostFields
    {
        /// <summary>
        /// The post ID
        /// </summary>
        Id,
        /// <summary>
        /// The type of post
        /// </summary>
        PostType,
        /// <summary>
        /// The category of the post
        /// </summary>
        Category,
        /// <summary>
        /// The member who created the post
        /// </summary>
        Creator,
        /// <summary>
        /// The title of the post
        /// </summary>
        Title,
        /// <summary>
        /// The post's summary
        /// </summary>
        Summary,
        /// <summary>
        /// The timestamp for when the post was created
        /// </summary>
        CreationTime,
        /// <summary>
        /// Following, likes and available action
        /// </summary>
        RelationToViewer,
        /// <summary>
        /// Likes for the post
        /// </summary>
        Likes,
        /// <summary>
        /// Comments on the post
        /// </summary>
        Attachment,
        /// <summary>
        /// LinkedIn site URL to the post
        /// </summary>
        SiteGroupPostUrl
    }

    /// <summary>
    /// Specifies identifiers to indicate group's posts flags
    /// </summary>
    public enum LinkedInGroupPostFlag
    {
        /// <summary>
        /// Promotion flag
        /// </summary>
        Promotion,
        /// <summary>
        /// Job flag
        /// </summary>
        Job
    }

    /// <summary>
    /// Specifies identifiers to indicate member's role for group's posts query
    /// </summary>
    public enum LinkedInGroupPostRole
    {
        /// <summary>
        /// Role is not defined
        /// </summary>
        NotDefined,
        /// <summary>
        /// Creator of the post
        /// </summary>
        Creator,
        /// <summary>
        /// Commenter of the post
        /// </summary>
        Commenter,
        /// <summary>
        /// Follower of the post
        /// </summary>
        Follower
    }

    /// <summary>
    /// Specifies identifiers to indicate which fields of group post comment should be retrieved
    /// </summary>
    public enum LinkedInGroupPostCommentFields
    {
        /// <summary>
        /// The comment ID
        /// </summary>
        Id,
        /// <summary>
        /// The comment test
        /// </summary>
        Text,
        /// <summary>
        /// The member who created the comment
        /// </summary>
        Creator,
        /// <summary>
        /// The timestamp for when the comment was created
        /// </summary>
        CreationTime,
        /// <summary>
        /// Available action
        /// </summary>
        RelationToViewer,
    }

    /// <summary>
    /// Specifies identifiers to indicate the frequency at which the member receives group emails
    /// </summary>
    public enum LinkedInEmailDigestFrequency
    {
        /// <summary>
        /// No emails
        /// </summary>
        None,
        /// <summary>
        /// Daily emails
        /// </summary>
        Daily,
        /// <summary>
        /// Weekly emails
        /// </summary>
        Weekly
    }

    /// <summary>
    /// Specifies identifiers to indicate the type of compane update
    /// </summary>
    public enum LinkedInCompanyUpdateType
    {
        /// <summary>
        /// Profile update
        /// </summary>
        ProfileUpdate,
        /// <summary>
        /// Status update
        /// </summary>
        StatusUpdate,
        /// <summary>
        /// Job update
        /// </summary>
        JobUpdate,
        /// <summary>
        /// Person update
        /// </summary>
        PersonUpdate
    }

    /// <summary>
    /// Specifies identifiers to indicate invitation type
    /// </summary>
    public enum LinkedInInvitationType
    {
        /// <summary>
        /// Invitation by user ID
        /// </summary>
        InviteById,
        /// <summary>
        /// Invitation by user e-mail
        /// </summary>
        InviteByEmail
    }

    /// <summary>
    /// Specifies LinkedIn country
    /// </summary>
    public enum LinkedInCountries
    {
        /// <summary>
        /// No country
        /// </summary>
        None,
        ///
        ///Andorra
        ///
        Andorra,
        ///
        ///United Arab Emirates
        ///
        UnitedArabEmirates,
        ///
        ///Afghanistan
        ///
        Afghanistan,
        ///
        ///Antigua and Barbuda
        ///
        AntiguaAndBarbuda,
        ///
        ///Anguilla
        ///
        Anguilla,
        ///
        ///Albania
        ///
        Albania,
        ///
        ///Armenia
        ///
        Armenia,
        ///
        ///Angola
        ///
        Angola,
        ///
        ///Antarctica
        ///
        Antarctica,
        ///
        ///Argentina
        ///
        Argentina,
        ///
        ///American Samoa
        ///
        AmericanSamoa,
        ///
        ///Austria
        ///
        Austria,
        ///
        ///Australia
        ///
        Australia,
        ///
        ///Aruba
        ///
        Aruba,
        ///
        ///Aland Islands
        ///
        AlandIslands,
        ///
        ///Azerbaijan
        ///
        Azerbaijan,
        ///
        ///Bosnia and Herzegovina
        ///
        BosniaAndHerzegovina,
        ///
        ///Barbados
        ///
        Barbados,
        ///
        ///Bangladesh
        ///
        Bangladesh,
        ///
        ///Belgium
        ///
        Belgium,
        ///
        ///Burkina Faso
        ///
        BurkinaFaso,
        ///
        ///Bulgaria
        ///
        Bulgaria,
        ///
        ///Bahrain
        ///
        Bahrain,
        ///
        ///Burundi
        ///
        Burundi,
        ///
        ///Benin
        ///
        Benin,
        ///
        ///Saint Barthélemy
        ///
        SaintBarthelemy,
        ///
        ///Bermuda
        ///
        Bermuda,
        ///
        ///Brunei Darussalam
        ///
        BruneiDarussalam,
        ///
        ///Bolivia, Plurinational State of
        ///
        BoliviaPlurinationalStateOf,
        ///
        ///Bonaire, Sint Eustatius and Saba
        ///
        BonaireSintEustatiusAndSaba,
        ///
        ///Brazil
        ///
        Brazil,
        ///
        ///Bahamas
        ///
        Bahamas,
        ///
        ///Bhutan
        ///
        Bhutan,
        ///
        ///Bouvet Island
        ///
        BouvetIsland,
        ///
        ///Botswana
        ///
        Botswana,
        ///
        ///Belarus
        ///
        Belarus,
        ///
        ///Belize
        ///
        Belize,
        ///
        ///Canada
        ///
        Canada,
        ///
        ///Cocos (Keeling) Islands
        ///
        CocosIslands,
        ///
        ///Congo, the Democratic Republic of the
        ///
        CongoTheDemocraticRepublic,
        ///
        ///Central African Republic
        ///
        CentralAfricanRepublic,
        ///
        ///Congo
        ///
        Congo,
        ///
        ///Switzerland
        ///
        Switzerland,
        ///
        ///Côte d'Ivoire
        ///
        CoteDivoire,
        ///
        ///Cook Islands
        ///
        CookIslands,
        ///
        ///Chile
        ///
        Chile,
        ///
        ///Cameroon
        ///
        Cameroon,
        ///
        ///China
        ///
        China,
        ///
        ///Colombia
        ///
        Colombia,
        ///
        ///Costa Rica
        ///
        CostaRica,
        ///
        ///Cuba
        ///
        Cuba,
        ///
        ///Cape Verde
        ///
        CapeVerde,
        ///
        ///Curaçao
        ///
        Curacao,
        ///
        ///Christmas Island
        ///
        ChristmasIsland,
        ///
        ///Cyprus
        ///
        Cyprus,
        ///
        ///Czech Republic
        ///
        CzechRepublic,
        ///
        ///Germany
        ///
        Germany,
        ///
        ///Djibouti
        ///
        Djibouti,
        ///
        ///Denmark
        ///
        Denmark,
        ///
        ///Dominica
        ///
        Dominica,
        ///
        ///Dominican Republic
        ///
        DominicanRepublic,
        ///
        ///Algeria
        ///
        Algeria,
        ///
        ///Ecuador
        ///
        Ecuador,
        ///
        ///Estonia
        ///
        Estonia,
        ///
        ///Egypt
        ///
        Egypt,
        ///
        ///Western Sahara
        ///
        WesternSahara,
        ///
        ///Eritrea
        ///
        Eritrea,
        ///
        ///Spain
        ///
        Spain,
        ///
        ///Ethiopia
        ///
        Ethiopia,
        ///
        ///Finland
        ///
        Finland,
        ///
        ///Fiji
        ///
        Fiji,
        ///
        ///Falkland Islands (Malvinas)
        ///
        FalklandIslands,
        ///
        ///Micronesia, Federated States of
        ///
        MicronesiaFederatedStatesOf,
        ///
        ///Faroe Islands
        ///
        FaroeIslands,
        ///
        ///France
        ///
        France,
        ///
        ///Gabon
        ///
        Gabon,
        ///
        ///United Kingdom
        ///
        UnitedKingdom,
        ///
        ///Grenada
        ///
        Grenada,
        ///
        ///Georgia
        ///
        Georgia,
        ///
        ///French Guiana
        ///
        FrenchGuiana,
        ///
        ///Guernsey
        ///
        Guernsey,
        ///
        ///Ghana
        ///
        Ghana,
        ///
        ///Gibraltar
        ///
        Gibraltar,
        ///
        ///Greenland
        ///
        Greenland,
        ///
        ///Gambia
        ///
        Gambia,
        ///
        ///Guinea
        ///
        Guinea,
        ///
        ///Guadeloupe
        ///
        Guadeloupe,
        ///
        ///Equatorial Guinea
        ///
        EquatorialGuinea,
        ///
        ///Greece
        ///
        Greece,
        ///
        ///South Georgia and the South Sandwich Islands
        ///
        SouthGeorgiaAndTheSouthSandwichIslands,
        ///
        ///Guatemala
        ///
        Guatemala,
        ///
        ///Guam
        ///
        Guam,
        ///
        ///Guinea-Bissau
        ///
        GuineaBissau,
        ///
        ///Guyana
        ///
        Guyana,
        ///
        ///Hong Kong
        ///
        HongKong,
        ///
        ///Heard Island and McDonald Islands
        ///
        HeardIslandAndMcDonaldIslands,
        ///
        ///Honduras
        ///
        Honduras,
        ///
        ///Croatia
        ///
        Croatia,
        ///
        ///Haiti
        ///
        Haiti,
        ///
        ///Hungary
        ///
        Hungary,
        ///
        ///Indonesia
        ///
        Indonesia,
        ///
        ///Ireland
        ///
        Ireland,
        ///
        ///Israel
        ///
        Israel,
        ///
        ///Isle of Man
        ///
        IsleOfMan,
        ///
        ///India
        ///
        India,
        ///
        ///British Indian Ocean Territory
        ///
        BritishIndianOceanTerritory,
        ///
        ///Iraq
        ///
        Iraq,
        ///
        ///Iran, Islamic Republic of
        ///
        IranIslamicRepublicOf,
        ///
        ///Iceland
        ///
        Iceland,
        ///
        ///Italy
        ///
        Italy,
        ///
        ///Jersey
        ///
        Jersey,
        ///
        ///Jamaica
        ///
        Jamaica,
        ///
        ///Jordan
        ///
        Jordan,
        ///
        ///Japan
        ///
        Japan,
        ///
        ///Kenya
        ///
        Kenya,
        ///
        ///Kyrgyzstan
        ///
        Kyrgyzstan,
        ///
        ///Cambodia
        ///
        Cambodia,
        ///
        ///Kiribati
        ///
        Kiribati,
        ///
        ///Comoros
        ///
        Comoros,
        ///
        ///Saint Kitts and Nevis
        ///
        SaintKittsAndNevis,
        ///
        ///Korea, Democratic People's Republic of
        ///
        KoreaDemocraticPeopleRepublicOf,
        ///
        ///Korea, Republic of
        ///
        KoreaRepublicOf,
        ///
        ///Kuwait
        ///
        Kuwait,
        ///
        ///Cayman Islands
        ///
        CaymanIslands,
        ///
        ///Kazakhstan
        ///
        Kazakhstan,
        ///
        ///Lao People's Democratic Republic
        ///
        LaoPeopleDemocraticRepublic,
        ///
        ///Lebanon
        ///
        Lebanon,
        ///
        ///Saint Lucia
        ///
        SaintLucia,
        ///
        ///Liechtenstein
        ///
        Liechtenstein,
        ///
        ///Sri Lanka
        ///
        SriLanka,
        ///
        ///Liberia
        ///
        Liberia,
        ///
        ///Lesotho
        ///
        Lesotho,
        ///
        ///Lithuania
        ///
        Lithuania,
        ///
        ///Luxembourg
        ///
        Luxembourg,
        ///
        ///Latvia
        ///
        Latvia,
        ///
        ///Libya
        ///
        Libya,
        ///
        ///Morocco
        ///
        Morocco,
        ///
        ///Monaco
        ///
        Monaco,
        ///
        ///Moldova, Republic of
        ///
        MoldovaRepublicOf,
        ///
        ///Montenegro
        ///
        Montenegro,
        ///
        ///Saint Martin (French part)
        ///
        SaintMartinFrenchPart,
        ///
        ///Madagascar
        ///
        Madagascar,
        ///
        ///Marshall Islands
        ///
        MarshallIslands,
        ///
        ///Macedonia, the former Yugoslav Republic of
        ///
        MacedoniaTheFormerYugoslavRepublicOf,
        ///
        ///Mali
        ///
        Mali,
        ///
        ///Myanmar
        ///
        Myanmar,
        ///
        ///Mongolia
        ///
        Mongolia,
        ///
        ///Macao
        ///
        Macao,
        ///
        ///Northern Mariana Islands
        ///
        NorthernMarianaIslands,
        ///
        ///Martinique
        ///
        Martinique,
        ///
        ///Mauritania
        ///
        Mauritania,
        ///
        ///Montserrat
        ///
        Montserrat,
        ///
        ///Malta
        ///
        Malta,
        ///
        ///Mauritius
        ///
        Mauritius,
        ///
        ///Maldives
        ///
        Maldives,
        ///
        ///Malawi
        ///
        Malawi,
        ///
        ///Mexico
        ///
        Mexico,
        ///
        ///Malaysia
        ///
        Malaysia,
        ///
        ///Mozambique
        ///
        Mozambique,
        ///
        ///Namibia
        ///
        Namibia,
        ///
        ///New Caledonia
        ///
        NewCaledonia,
        ///
        ///Niger
        ///
        Niger,
        ///
        ///Norfolk Island
        ///
        NorfolkIsland,
        ///
        ///Nigeria
        ///
        Nigeria,
        ///
        ///Nicaragua
        ///
        Nicaragua,
        ///
        ///Netherlands
        ///
        Netherlands,
        ///
        ///Norway
        ///
        Norway,
        ///
        ///Nepal
        ///
        Nepal,
        ///
        ///Nauru
        ///
        Nauru,
        ///
        ///Niue
        ///
        Niue,
        ///
        ///New Zealand
        ///
        NewZealand,
        ///
        ///Oman
        ///
        Oman,
        ///
        ///Panama
        ///
        Panama,
        ///
        ///Peru
        ///
        Peru,
        ///
        ///French Polynesia
        ///
        FrenchPolynesia,
        ///
        ///Papua New Guinea
        ///
        PapuaNewGuinea,
        ///
        ///Philippines
        ///
        Philippines,
        ///
        ///Pakistan
        ///
        Pakistan,
        ///
        ///Poland
        ///
        Poland,
        ///
        ///Saint Pierre and Miquelon
        ///
        SaintPierreAndMiquelon,
        ///
        ///Pitcairn
        ///
        Pitcairn,
        ///
        ///Puerto Rico
        ///
        PuertoRico,
        ///
        ///Palestine, State of
        ///
        PalestineStateOf,
        ///
        ///Portugal
        ///
        Portugal,
        ///
        ///Palau
        ///
        Palau,
        ///
        ///Paraguay
        ///
        Paraguay,
        ///
        ///Qatar
        ///
        Qatar,
        ///
        ///Réunion
        ///
        Reunion,
        ///
        ///Romania
        ///
        Romania,
        ///
        ///Serbia
        ///
        Serbia,
        ///
        ///Russian Federation
        ///
        RussianFederation,
        ///
        ///Rwanda
        ///
        Rwanda,
        ///
        ///Saudi Arabia
        ///
        SaudiArabia,
        ///
        ///Solomon Islands
        ///
        SolomonIslands,
        ///
        ///Seychelles
        ///
        Seychelles,
        ///
        ///Sudan
        ///
        Sudan,
        ///
        ///Sweden
        ///
        Sweden,
        ///
        ///Singapore
        ///
        Singapore,
        ///
        ///Saint Helena, Ascension and Tristan da Cunha
        ///
        SaintHelenaAscensionAndTristanDaCunha,
        ///
        ///Slovenia
        ///
        Slovenia,
        ///
        ///Svalbard and Jan Mayen
        ///
        SvalbardAndJanMayen,
        ///
        ///Slovakia
        ///
        Slovakia,
        ///
        ///Sierra Leone
        ///
        SierraLeone,
        ///
        ///San Marino
        ///
        SanMarino,
        ///
        ///Senegal
        ///
        Senegal,
        ///
        ///Somalia
        ///
        Somalia,
        ///
        ///Suriname
        ///
        Suriname,
        ///
        ///South Sudan
        ///
        SouthSudan,
        ///
        ///Sao Tome and Principe
        ///
        SaoTomeAndPrincipe,
        ///
        ///El Salvador
        ///
        ElSalvador,
        ///
        ///Sint Maarten (Dutch part)
        ///
        SintMaartenDutchPart,
        ///
        ///Syrian Arab Republic
        ///
        SyrianArabRepublic,
        ///
        ///Swaziland
        ///
        Swaziland,
        ///
        ///Turks and Caicos Islands
        ///
        TurksAndCaicosIslands,
        ///
        ///Chad
        ///
        Chad,
        ///
        ///French Southern Territories
        ///
        FrenchSouthernTerritories,
        ///
        ///Togo
        ///
        Togo,
        ///
        ///Thailand
        ///
        Thailand,
        ///
        ///Tajikistan
        ///
        Tajikistan,
        ///
        ///Tokelau
        ///
        Tokelau,
        ///
        ///Timor-Leste
        ///
        TimorLeste,
        ///
        ///Turkmenistan
        ///
        Turkmenistan,
        ///
        ///Tunisia
        ///
        Tunisia,
        ///
        ///Tonga
        ///
        Tonga,
        ///
        ///Turkey
        ///
        Turkey,
        ///
        ///Trinidad and Tobago
        ///
        TrinidadAndTobago,
        ///
        ///Tuvalu
        ///
        Tuvalu,
        ///
        ///Taiwan, Province of China
        ///
        TaiwanProvinceOfChina,
        ///
        ///Tanzania, United Republic of
        ///
        TanzaniaUnitedRepublicOf,
        ///
        ///Ukraine
        ///
        Ukraine,
        ///
        ///Uganda
        ///
        Uganda,
        ///
        ///United States Minor Outlying Islands
        ///
        UnitedStatesMinorOutlyingIslands,
        ///
        ///United States
        ///
        UnitedStates,
        ///
        ///Uruguay
        ///
        Uruguay,
        ///
        ///Uzbekistan
        ///
        Uzbekistan,
        ///
        ///Holy See (Vatican City State)
        ///
        VaticanCityState,
        ///
        ///Saint Vincent and the Grenadines
        ///
        SaintVincentAndTheGrenadines,
        ///
        ///Venezuela, Bolivarian Republic of
        ///
        VenezuelaBolivarianRepublicOf,
        ///
        ///Virgin Islands, British
        ///
        VirginIslandsBritish,
        ///
        ///Virgin Islands, U.S.
        ///
        VirginIslandsUS,
        ///
        ///Viet Nam
        ///
        VietNam,
        ///
        ///Vanuatu
        ///
        Vanuatu,
        ///
        ///Wallis and Futuna
        ///
        WallisAndFutuna,
        ///
        ///Samoa
        ///
        Samoa,
        ///
        ///Yemen
        ///
        Yemen,
        ///
        ///Mayotte
        ///
        Mayotte,
        ///
        ///South Africa
        ///
        SouthAfrica,
        ///
        ///Zambia
        ///
        Zambia,
        ///
        ///Zimbabwe
        ///
        Zimbabwe
    }
}
