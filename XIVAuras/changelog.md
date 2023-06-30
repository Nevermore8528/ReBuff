# Version 0.3.0.5
Add multi-indicators. These are indicators that light up based on how full a gauge is. Each unit is one indicator.
- Orientation, spread and size is completely controllable. There are also multiple shape options avaliable.
All Indicators have been combined into the Bar widget.
- "Icon" widgets have been left mostly untouched and previously added options were moved into the "Bar" widget's configurations to help maintain compatibility with older XIVAuras configs.
Add Job Gauges as Bar data sources and trigger data.
- Some job data might not be accurate, I don't play many of these jobs so I cannot test them. Please report any errors!
Started work on Lost Actions, only avaliable as a true/false trigger option for the player currently.

Known issues:
- Due to a change in the configuration options, some older Bar Widgets may inaccurately switch their data source to "HP" instead of "Time" or "Stacks" this is fixed by just setting the option again. This only affects Bar widgets made prior to this release.
- Chevron style indicators may have slight graphic issues at certain resolutions or when the indicator has transparency, this is an imgui limitation. (If anyone knows how to combine two draw objects in a way that they will not overlap their transparencies, I'm welcoming PRs!)
- Rounded Indicators have less depth when outlines are enabled, this is an imgui limitation.
- When many elements are created, there may be noticable frame drops. This probably isn't fixable tbh

# Version 0.3.0.4
- Attempt compatibility fix for existing XIVAuras presets
- Job/Class datasource removed pending bugfixes
- Party Member triggers removed pending bugfixes

# Version 0.3.0.2
- Update for 6.4
- Fix desaturated icon toggle

# Version 0.3.0.0
- Project taken over and premise reworked in preperation for main repo approval.
- Dynamic Bars added; includes rounded, vertical and multicolor variants.
- Diamond Indicators added.
- Job/Class datasource added.
- Party Member triggers added. (Needs testing)
- AutoSort function added to continually sort auras in a group as they become visible.

# To Do: 
	Validate what data can be obtained safely from party members
