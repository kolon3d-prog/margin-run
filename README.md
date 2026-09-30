# Margin Run

notebook → 3d parkour

you draw lines on paper like in a sketchbook, hit a key, they become walls, and then you run through what you just drew. me and my friend kent are making this - first real game for both of us. shipping for hack club third space.

the vibe is crumpled xerox / pencil notebook. not clean ahh shit. paper floors, drawn geometry, corridor that feels like the edge of a page.

## how to play

open `SampleScene` and `Drawing` together in the editor, press Play.

1. **T** - sketch mode (player off, paper on)
2. click the pencil button, draw on the paper
3. **R** - extrudes your lines into 3d, hides the drawing ui, turns the player on
4. run / jump around what you made
5. fall off the map (y < -20) and you go back to sketch instead of softlocking the scenes

### controls

- **T** sketch
- **R** extrude + run
- **WASD** move
- **mouse** look
- **space** jump
- pencil / eraser buttons on the paper ui
- undo / redo up top if you mess up a stroke
- **[ / ]** thinner / thicker pencil (or use the slider)
- **F** inspect the karambit

movement is kinda source-like (wishdir, air accel, friction). feels bad, but at least it's something. definitely should work on it in next weeks.

## team

- **me (kolon3d)** - movement, 3d, scene glue, the stupid bugs at 2am
- **mkvinnnn** - 2d editor side + art.

## week 1 ship

- 2d paper drawer (lines, undo/redo, eraser)
- line → prism extrude into the run scene(it's weird)
- sketch / run mode switch so youre not drawing on top of the 3d player
- multi-scene glue (drawing + sample scene). inspector drag between scenes is broken so we find stuff at runtime
- death used to reload one scene and murder DrawingRoot. now it teleports + goes back to sketch
- parkour pieces parented under one empty so we can rotate the whole level without breaking the drawer (dont rotate DrawingRoot. trust me)

working loop: draw → extrude → run → die → draw again.

its rough. orientation between paper and parkour took forever. a lot of learning by breaking things.

## week 2 ship

this week was mostly making it feel more like an actual game instead of a tech demo.

- pencil thickness - theres a slider now (and [ ] keys). thin pencil = thin bridge, thick pencil = wide platform. so how you draw actually matters for the run
- camera fly-in - when you hit run the camera flies from the notebook view straight into the players head. no more instant cut, it feels way smoother
- pencil outlines on platforms - extruded blocks get the dark pencil outline now so they look drawn, not like grey unity cubes
- i broke and fixed materials this week lol
- character in sketch mode - little leaf guy standing at the spawn with a looping idle. hand drawn frames
- karambit - yes. hand drawn, press F to inspect like in cs. its in the bottom right while you run. i think it's beautiful
- small stuff: cursor locks/unlocks properly between modes, sketch mode starts correctly on boot

still rough. movement still needs work and theres no finish line yet, thats next.

i used ai for help this week (explaining stuff and some snippets), wrote and wired most of it by hand in unity

## tech

unity 6, urp, c#. scripts live under `Assets/scripts` (movement, modesketch, runExtrude, lineExtruder, Drawing/*).
