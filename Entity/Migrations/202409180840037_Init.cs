namespace Entity
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(unicode: false),
                        Password = c.String(unicode: false),
                        Role = c.String(unicode: false),
                        CreatedDate = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BarcodeScans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Barcode = c.String(unicode: false),
                        ScanTime = c.DateTime(nullable: false, precision: 0),
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
                        Name = c.String(unicode: false),
                        Model = c.String(unicode: false),
                        CreatedDate = c.DateTime(nullable: false, precision: 0),
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
