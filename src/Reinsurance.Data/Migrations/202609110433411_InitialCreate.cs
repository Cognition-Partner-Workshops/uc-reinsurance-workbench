namespace Reinsurance.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TreatyLayers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TreatyId = c.Int(nullable: false),
                        LayerNumber = c.Int(nullable: false),
                        Limit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Attachment = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Reinstatements = c.Int(nullable: false),
                        ReinstatementPremiumPct = c.Decimal(nullable: false, precision: 9, scale: 6),
                        SharePct = c.Decimal(nullable: false, precision: 9, scale: 6),
                        Currency = c.String(nullable: false, maxLength: 10),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Treaties", t => t.TreatyId)
                .Index(t => t.TreatyId);
            
            CreateTable(
                "dbo.Treaties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubmissionId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 250),
                        Type = c.Int(nullable: false),
                        Currency = c.String(nullable: false, maxLength: 10),
                        Status = c.Int(nullable: false),
                        InceptionDate = c.DateTime(nullable: false),
                        ExpiryDate = c.DateTime(nullable: false),
                        CreatedOnUtc = c.DateTime(nullable: false),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Submissions", t => t.SubmissionId)
                .Index(t => t.SubmissionId);
            
            CreateTable(
                "dbo.Submissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Reference = c.String(nullable: false, maxLength: 100),
                        CedentId = c.Int(nullable: false),
                        BrokerId = c.Int(),
                        UnderwriterId = c.Int(),
                        ReceivedOn = c.DateTime(nullable: false),
                        InceptionDate = c.DateTime(nullable: false),
                        ExpiryDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        Notes = c.String(maxLength: 2000),
                        Deleted = c.Boolean(nullable: false),
                        CreatedOnUtc = c.DateTime(nullable: false),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Brokers", t => t.BrokerId)
                .ForeignKey("dbo.Cedents", t => t.CedentId)
                .ForeignKey("dbo.Underwriters", t => t.UnderwriterId)
                .Index(t => t.Reference, unique: true, name: "UX_Submissions_Reference")
                .Index(t => t.CedentId)
                .Index(t => t.BrokerId)
                .Index(t => t.UnderwriterId);
            
            CreateTable(
                "dbo.Brokers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Code = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CatModelResults",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubmissionId = c.Int(nullable: false),
                        RegionId = c.Int(),
                        PerilId = c.Int(),
                        ModelVendor = c.String(nullable: false, maxLength: 100),
                        ModelVersion = c.String(nullable: false, maxLength: 50),
                        AAL = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ExpectedLoss = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PML50 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PML100 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PML250 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RunOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Submissions", t => t.SubmissionId)
                .Index(t => t.SubmissionId);
            
            CreateTable(
                "dbo.Cedents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Code = c.String(nullable: false, maxLength: 50),
                        Country = c.String(maxLength: 100),
                        Rating = c.String(maxLength: 10),
                        Active = c.Boolean(nullable: false),
                        CreatedOnUtc = c.DateTime(nullable: false),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true, name: "UX_Cedents_Code");
            
            CreateTable(
                "dbo.ExposureRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubmissionId = c.Int(nullable: false),
                        RegionId = c.Int(nullable: false),
                        PerilId = c.Int(nullable: false),
                        TotalInsuredValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RiskCount = c.Int(nullable: false),
                        AverageDeductiblePct = c.Decimal(nullable: false, precision: 9, scale: 6),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Perils", t => t.PerilId)
                .ForeignKey("dbo.Regions", t => t.RegionId)
                .ForeignKey("dbo.Submissions", t => t.SubmissionId)
                .Index(t => t.SubmissionId)
                .Index(t => t.RegionId)
                .Index(t => t.PerilId);
            
            CreateTable(
                "dbo.Perils",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 20),
                        Name = c.String(nullable: false, maxLength: 200),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Regions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 30),
                        Name = c.String(nullable: false, maxLength: 200),
                        CountryCode = c.String(nullable: false, maxLength: 10),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Underwriters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Email = c.String(nullable: false, maxLength: 200),
                        AuthorityLimit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PortfolioLimits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegionId = c.Int(),
                        PerilId = c.Int(),
                        MaxAggregateLimit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxPML250 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ReferralRules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false, maxLength: 500),
                        Active = c.Boolean(nullable: false),
                        Kind = c.Int(nullable: false),
                        Threshold = c.Decimal(precision: 18, scale: 2),
                        TextValue = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true, name: "UX_ReferralRules_Code");
            
            CreateTable(
                "dbo.PricingResults",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TreatyLayerId = c.Int(nullable: false),
                        TechnicalPremium = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ExpectedLoss = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ExpenseLoad = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RiskLoad = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RateOnLine = c.Decimal(nullable: false, precision: 9, scale: 6),
                        LossCostPct = c.Decimal(nullable: false, precision: 9, scale: 6),
                        ProfitMarginPct = c.Decimal(nullable: false, precision: 9, scale: 6),
                        ReferralRequired = c.Boolean(nullable: false),
                        ReferralReasons = c.String(maxLength: 2000),
                        CalculatedOn = c.DateTime(nullable: false),
                        CreatedOnUtc = c.DateTime(nullable: false),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TreatyLayers", t => t.TreatyLayerId)
                .Index(t => t.TreatyLayerId);
            
            CreateTable(
                "dbo.LossEvents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CedentId = c.Int(nullable: false),
                        RegionId = c.Int(nullable: false),
                        PerilId = c.Int(nullable: false),
                        EventName = c.String(nullable: false, maxLength: 250),
                        LossDate = c.DateTime(nullable: false),
                        GroundUpLoss = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CededLoss = c.Decimal(precision: 18, scale: 2),
                        TreatyId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PricingResults", "TreatyLayerId", "dbo.TreatyLayers");
            DropForeignKey("dbo.TreatyLayers", "TreatyId", "dbo.Treaties");
            DropForeignKey("dbo.Treaties", "SubmissionId", "dbo.Submissions");
            DropForeignKey("dbo.Submissions", "UnderwriterId", "dbo.Underwriters");
            DropForeignKey("dbo.ExposureRecords", "SubmissionId", "dbo.Submissions");
            DropForeignKey("dbo.ExposureRecords", "RegionId", "dbo.Regions");
            DropForeignKey("dbo.ExposureRecords", "PerilId", "dbo.Perils");
            DropForeignKey("dbo.Submissions", "CedentId", "dbo.Cedents");
            DropForeignKey("dbo.CatModelResults", "SubmissionId", "dbo.Submissions");
            DropForeignKey("dbo.Submissions", "BrokerId", "dbo.Brokers");
            DropIndex("dbo.PricingResults", new[] { "TreatyLayerId" });
            DropIndex("dbo.ReferralRules", "UX_ReferralRules_Code");
            DropIndex("dbo.ExposureRecords", new[] { "PerilId" });
            DropIndex("dbo.ExposureRecords", new[] { "RegionId" });
            DropIndex("dbo.ExposureRecords", new[] { "SubmissionId" });
            DropIndex("dbo.Cedents", "UX_Cedents_Code");
            DropIndex("dbo.CatModelResults", new[] { "SubmissionId" });
            DropIndex("dbo.Submissions", new[] { "UnderwriterId" });
            DropIndex("dbo.Submissions", new[] { "BrokerId" });
            DropIndex("dbo.Submissions", new[] { "CedentId" });
            DropIndex("dbo.Submissions", "UX_Submissions_Reference");
            DropIndex("dbo.Treaties", new[] { "SubmissionId" });
            DropIndex("dbo.TreatyLayers", new[] { "TreatyId" });
            DropTable("dbo.LossEvents");
            DropTable("dbo.PricingResults");
            DropTable("dbo.ReferralRules");
            DropTable("dbo.PortfolioLimits");
            DropTable("dbo.Underwriters");
            DropTable("dbo.Regions");
            DropTable("dbo.Perils");
            DropTable("dbo.ExposureRecords");
            DropTable("dbo.Cedents");
            DropTable("dbo.CatModelResults");
            DropTable("dbo.Brokers");
            DropTable("dbo.Submissions");
            DropTable("dbo.Treaties");
            DropTable("dbo.TreatyLayers");
        }
    }
}
