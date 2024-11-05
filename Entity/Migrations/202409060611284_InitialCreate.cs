namespace Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Password = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BarcodeScans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Barcode = c.String(),
                        ScanTime = c.DateTime(nullable: false),
                        AccountId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BarcodeScans", "AccountId", "dbo.Accounts");
            DropIndex("dbo.BarcodeScans", new[] { "AccountId" });
            DropTable("dbo.BarcodeScans");
            DropTable("dbo.Accounts");
        }
    }
}
