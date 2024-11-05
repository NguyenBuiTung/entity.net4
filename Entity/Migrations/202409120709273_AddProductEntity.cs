namespace Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductEntity : DbMigration
    {
        public override void Up()
        {
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
            
            AddColumn("dbo.BarcodeScans", "ProductId", c => c.Int(nullable: false));
            CreateIndex("dbo.BarcodeScans", "ProductId");
            AddForeignKey("dbo.BarcodeScans", "ProductId", "dbo.Products", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BarcodeScans", "ProductId", "dbo.Products");
            DropIndex("dbo.BarcodeScans", new[] { "ProductId" });
            DropColumn("dbo.BarcodeScans", "ProductId");
            DropTable("dbo.Products");
        }
    }
}
