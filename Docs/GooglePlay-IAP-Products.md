# Tile Sky - 7 Google Play IAP products

App package: com.chp.tilesky

Use these existing Product IDs exactly. They already match the Unity catalog, shop buttons and reward mappings. Prices below are reference USD prices from the local catalog; configure actual prices and region availability in Play Console.

| Name | Product ID | Unity product type | Reference USD price |
| --- | --- | --- | --- |
| No Ads | tilesky_removeads | Non-consumable (permanent) | $3.90 |
| Starter Pack | tilesky_firstpack | Consumable | $2.90 |
| Mega Pack | tilesky_megapack | Consumable | $4.90 |
| Ultimate Pack | tilesky_ultipack | Consumable | $9.90 |
| Gold Pack 1 | tilesky_gold1 | Consumable | $4.90 |
| Gold Pack 2 | tilesky_gold2 | Consumable | $12.90 |
| Gold Pack 3 | tilesky_gold3 | Consumable | $29.90 |

Create these as one-time products, not subscriptions. Enter the Product ID, localized name/description, purchase option, regional prices and availability; activate when the Play Console requirements are satisfied. Consumable versus non-consumable behavior is handled by the billing integration. Product IDs cannot be renamed after creation.

Current project status: PurchasingEnabled is false, catalog auto-initialization is disabled, and serialized IAP components are disabled. Creating products on Play Console alone does not enable purchases in the game. Store initialization, purchase/reward handling, restoration and Play test-track validation still need to be enabled and tested before release.

The catalog also has Piggy Bank and Battle Pass entries. They are not part of these seven shop products.

Official guide: https://support.google.com/googleplay/android-developer/answer/1153481