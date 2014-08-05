Higher Order Programming Environment
=========

The Higher Order Programming Environment Concept
---

The Higher Order Programming Environment is an architectural template for implementing end-user processes as finite automata (FA) in a 
distributed computing space.  FA's inter-communicate with semanticized data, enabling the end-user to create unique and custom 
computational stacks out of FA building blocks, called "receptors", which self-wire based on the semantic "interest" of each receptor 
in the semantic information emitted by other receptors (similar to a pub-sub architecture.)  Applications developed in the HOPE architecture 
are continually emergent in that new semantic types (and therefore concrete meaning) can be created from existing data and new computational 
stacks can be constructed from existing and/or new receptors to work with new meaning (information.)  Unique ways of rendering information 
can be immediately plugged into an existing computation stack, as rending, being just another kind of computation, is implemented as a receptor.  
Text-to-speech, 2D and 3D modeling, language translation, charting, these are all ways that a user can dynamically create specialized user 
interfaces for their individual needs.

HOPE Application Development
---

Application development occurs in several tiers.  The underlying framework is open sourced, whereas receptor development can be either 
contributed to the open source community or the receptor assembly sold commercially or held privately by individuals or corporations for their 
proprietary use.  The framework and receptors are developed in the traditional software development process with programmers.  Conversely, 
many HOPE application stacks can be implemented by users with little training, although complex stacks often utilize specialized application 
stack consultants.  Being an open architecture, the end-user can also contract or hire in-house receptor developers, either for commercial 
purposes or for propriety in-house algorithms.  Regardless, all communities can draw on the growing number of open-source receptors either 
to use directly or to customize for their own purposes.

HOPE Receptors
---

Receptors are semantic finite automata.  As in a pub-sub architecture, receptors inform a broker of semantic data in which the 
receptor has interest.  Semantic information that is published by a receptor is automatically distributed to subscribing receptors.  
However, the receptor can qualify acceptance of the information using non-static filters (dynamic filtering allows receptors to adjust for 
overall internal and external system state.)  Furthermore, the broker can "emit" sub-components of a semantic structure to interested 
subscriber receptors when the sub-component is itself semantic (as opposed to a native computer type.)  This enables unique computational stacks 
on semantic subsets without having to specifically break apart the semantic structure into its components. 

Screenshots
----

* An APOD website scraper applet:

![APOD Viewer](http://www.codeproject.com/KB/cs/781135/M33.png)

* Membrane Computing (release 6-9-2014):

![Membrane Computing](https://marcclifton.files.wordpress.com/2014/06/membranes.png)

* Natural Language Processing of RSS Feeds

![NLP](http://www.codeproject.com/KB/cs/797457/filter3.png)

Demos
----

Watch the videos:

 - [Introduction]
 - [An APOD Website Scraper]
 - [Membrane Computing Video]
 - [Hunt the Wumpus Video]

For Programmers
----

Read the articles:

 - [Introductory Article]
 - [APOD Scraper Article] 
 - [Hunt the Wumpus]
 - [Semantic Web and Natural Language Processing]

To Contribute
----

We are actively looking for developers interested in:

1. Expanding our library of receptors.  Some ideas: Email "reader", stock quotes reader, news feed, etc.
2. Porting HOPE to a web application supporting both desktop and mobile devices
3. Improving on visualizations
4. Developing applets from receptors

Marketing / Funding
----

We are actively seeking to create interest in a variety of markets, including education, finance, presentation, and information management.  We are also seeking investors for both the open source implementation and in the development of the commercial, revenue-generating arm.

Workshops / Training Tools
----

Workshops will be schelued and specific training tools will available after the initial development cycle is complete.

License
----

MIT

[Introduction]:http://youtu.be/O1V4XSYYNxs
[An APOD Website Scraper]:http://youtu.be/NdapAL2tt7w
[Membrane Computing Video]:http://youtu.be/XoQSTJcrEj8
[Hunt the Wumpus Video]:https://www.youtube.com/watch?v=llRUw9pX7bQ&feature=youtu.be
[Introductory Article]: http://www.codeproject.com/Articles/777843/HOPE-Higher-Order-Programming-Environment
[APOD Scraper Article]:http://www.codeproject.com/Articles/781135/APOD-Website-Scraper-a-HOPE-demonstration
[Membrane Computing]:http://en.wikipedia.org/wiki/Membrane_computing
[Hunt the Wumpus]:http://www.codeproject.com/Articles/786118/Hunt-the-Wumpus
[Semantic Web and Natural Language Processing]:http://www.codeproject.com/Articles/797457/The-Semantic-Web-and-Natural-Language-Processing

