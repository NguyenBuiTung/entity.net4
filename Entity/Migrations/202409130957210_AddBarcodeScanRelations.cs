namespace Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBarcodeScanRelations : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Accounts", "Username", c => c.String(nullable: false));
            AlterColumn("dbo.Accounts", "Password", c => c.String(nullable: false));
            AlterColumn("dbo.BarcodeScans", "Barcode", c => c.String(nullable: false));
            AlterColumn("dbo.Products", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Products", "Model", c => c.String(nullable: false));
            CreateIndex("dbo.BarcodeScans", "Barcode", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.BarcodeScans", new[] { "Barcode" });
            AlterColumn("dbo.Products", "Model", c => c.String());
            AlterColumn("dbo.Products", "Name", c => c.String());
            AlterColumn("dbo.BarcodeScans", "Barcode", c => c.String());
            AlterColumn("dbo.Accounts", "Password", c => c.String());
            AlterColumn("dbo.Accounts", "Username", c => c.String());
        }
    }
}
