# Leaderboard Configuration assets

A Leaderboard Configuration asset defines the configuraiton of a leaderboard in the Leaderboards service.

A Leaderboard Configuration file has `.lb` for extension.

## Creation

Right-click on the `Project Window` then select `Create > Services > Leaderboard Configuration` to create a Leaderboard Configuration asset.

Once created, a corresponding item will appear in the deployment window, and will allow you to deploy the newly created Leaderboard Configuration file.

## Edition

Any text editor can modify a Leaderboard Configuration file, however, it is always preferred to choose an IDE that [supports JSON Schema definitions](https://json-schema.org/implementations#editors) to benefit from code completion.

## Deletion

Deleting a Leaderboard Configuration asset isn't enough to remove the resouce from the service, it is also needed to delete it using the Unity dashboard.

## Format and schema

A Leaderboard Configuration asset is written in JSON, and its schema is defined [here](https://ugs-config-schemas.unity3d.com/v1/leaderboards.schema.json).

Here is an example of a Leaderboard Configuration asset content.

```json
{
  "SortOrder": "asc",
  "UpdateType": "keepBest",
  "ResetConfig": {
    "Start": "2023-12-24T00:00:00-05:00",
    "Schedule": "0 12 1 * *"
  },
  "TieringConfig": {
    "Strategy": "score",
    "Tiers": [
      {
        "Id": "Gold",
        "Cutoff": 200.0
      },
      {
        "Id": "Silver",
        "Cutoff": 100.0
      },
      {
        "Id": "Bronze"
      }
    ]
  },
  "Name": "My Leaderboard Config"
}
```

## Naming Restrictions

The name of the created file will be used as the ID for the corresponding Leaderboard, therefore it can only contain letters, numbers, dashes, and underscores.
