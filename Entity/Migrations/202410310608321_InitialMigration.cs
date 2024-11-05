namespace Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
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
                        Role = c.String(),
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
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.AccountId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Model = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BarcodeScans", "ProductId", "dbo.Products");
            DropForeignKey("dbo.BarcodeScans", "AccountId", "dbo.Accounts");
            DropIndex("dbo.BarcodeScans", new[] { "ProductId" });
            DropIndex("dbo.BarcodeScans", new[] { "AccountId" });
            DropTable("dbo.Products");
            DropTable("dbo.BarcodeScans");
            DropTable("dbo.Accounts");
        }
    }
}
