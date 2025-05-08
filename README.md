# TerrraiaInfiniteChests

This is an updated version of Zaicon Kiroshu's [InfiniteChestsV3 plugin](https://github.com/Zaicon/InfiniteChestsV3) (which was originally based on MarioE's InfiniteChests).
I have updated the plugin to work with the current version of Terraria and TShock 5.2.3.

## Plugin Features

- Saves chests to database, allowing more than 1,000 chests per map
- Allows conversion of chest storage to/from database at will
- Allows chests to be "claimed" (protected) by users
- Allows chests to be "public" (other users can edit but not destroy)
- Allows chests to "refill" (chest contents are restored at a specified interval)
- Allows players to destroy all empty chests automatically
- Allows players to allow only certain users and/or groups to access protected chests
- Allows players to search the entire chest database for specific items
- Supports usage of Key of Light/Key of Night items

## Installation

1. Ensure you're running TShock 5.2.3 or newer on Terraria 1.4+
2. Ensure your server supports .NET 6.0
3. Download the latest release from the GitHub releases page
4. Place the `TerrraiaInfiniteChests.dll` in your server's `ServerPlugins` folder
5. Restart your TShock server
6. The plugin will automatically create the necessary database tables on first run

## Database Configuration

TerrraiaInfiniteChests uses the same database configuration as your TShock server:

- For SQLite, no additional configuration is needed
- For MySQL, ensure your MySQL credentials are properly configured in the TShock config

The plugin creates the following tables in your database:

- `InfiniteChests` - Stores chest data and protection information
- `InfiniteChestItems` - Stores the contents of each database chest

## Commands

```
/chest claim - Protects a chest via user account
/chest unclaim - Unprotects a chest
/chest info - Displays X/Y coordinates and account owner
/chest search <item name> - Searches chests for a specific item
/chest allow <player name> - Gives user access to chest
/chest remove <player name> - Removes chest access from user
/chest allowgroup <group name> - Gives players with that group access to chest
/chest removegroup <group name> - Removes group access to chest
/chest public - Toggles the 'public' setting of a chest, allowing others to use but not destroy the chest
/chest refill <seconds> - Sets the interval in which chests refill items
/chest cancel - Cancels any of the above actions
/convchests [-r] - Converts any "real" chests to database chests (or reverse with `-r`)
/prunechests - Permanently removes empty chests from the world and database
/transferchests - Converts the database from previous plugin versions
```

## Permissions

```
ic.use - Enables use of /chest
ic.claim - Enables use of /chest claim, unclaim
ic.info - Enables use of /chest info
ic.search - Enables use of /chest search
ic.public - Enables use of /chest public
ic.protect - Players with this permission will have their chests automatically protected via user account
ic.refill - Enables use of /chest refill
ic.edit - Allows player to edit any chest regardless of chest protection
ic.convert - Enables use of /convchests
ic.prune - Enables use of /prunechests
ic.transfer - Enables use of /transferchests
```

## Upgrading from Previous Versions

If you're upgrading from a previous version of InfiniteChests, follow these steps:

1. Back up your existing database
2. Install TerrraiaInfiniteChests as described in the Installation section
3. Run `/transferchests` to migrate your chest data
4. The migration process will transfer all chest data including protection settings
5. Once completed, verify that your chests are accessible in-game

_Note: The transfer process is one-way and cannot be reversed, which is why a backup is recommended._

## Chest Refill Feature

The refill feature allows chests to automatically restore their original contents after a specified time interval:

- When a chest is set to refill, the plugin records the chest's current contents as the "refill state"
- After the specified interval (in seconds), the chest contents will be reset to this state
- Items taken by players will reappear, and items added will be removed
- This is ideal for creating shops, dungeon loot, or resource chests that periodically replenish

To set a refill interval:

1. Fill the chest with the items you want it to contain
2. Use `/chest refill <seconds>` to set the interval
3. Use `/chest refill 0` to disable the refill feature

## Technical Details

- Chest data is stored in a relational database rather than the world file
- This bypasses Terraria's 1000 chest limit per world
- The plugin uses an asynchronous database handler to minimize impact on server performance
- When a player interacts with a chest, the plugin checks permissions before allowing access
- The refill system uses a timer that tracks each chest's refill state
- The plugin hooks into Terraria's chest mechanics to display and modify chest contents

## Usage Examples

### Creating a Shop Chest

1. Place a chest and fill it with items you want to sell
2. Use `/chest claim` to protect it
3. Use `/chest public` to make it accessible to others
4. Use `/chest refill 300` to make items reappear every 5 minutes

### Team Storage Chest

1. Place a chest for team storage
2. Use `/chest claim` to protect it
3. For each team member: `/chest allow <player>`
4. Or add by group: `/chest allowgroup <group>`

### Creating an Admin-Only Chest

1. Place a chest for admin items
2. Use `/chest claim` to protect it
3. Use `/chest allowgroup admin` to give admin group access

## Troubleshooting

### Chest Not Showing in Database

- Ensure the chest was placed after installing the plugin
- Try using `/convchests` to convert existing chests to database chests

### Permission Issues

- Check that players have the correct permissions
- Verify chest ownership with `/chest info`
- Ensure groups are spelled correctly when using `allowgroup`/`removegroup`

### Database Errors

- Check your database connection settings
- Ensure your database user has permission to create and modify tables
- For MySQL users, verify the character set is compatible

### Performance Issues

- Large numbers of database chests (10,000+) may cause lag during world load
- Consider using `/prunechests` periodically to remove empty chests
