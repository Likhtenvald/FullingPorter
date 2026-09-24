from PIL import Image, ImageDraw, ImageFilter

SIZE = 1024
SCALE = 4
im = Image.new("RGBA", (SIZE, SIZE), (12, 30, 27, 255))
d = ImageDraw.Draw(im)

# Rounded field and rim, with generous clear space for small mod-manager tiles.
d.rounded_rectangle((38, 38, 986, 986), radius=196, fill=(18, 48, 41, 255), outline=(204, 157, 74, 255), width=28)
d.rounded_rectangle((77, 77, 947, 947), radius=166, outline=(79, 119, 90, 255), width=10)

# A stylized Fuling head: ears, tuft, face and bright eyes.
d.polygon([(270, 385), (98, 294), (184, 490), (315, 481)], fill=(97, 145, 80, 255), outline=(205, 173, 92, 255), width=13)
d.polygon([(754, 385), (926, 294), (840, 490), (709, 481)], fill=(97, 145, 80, 255), outline=(205, 173, 92, 255), width=13)
d.ellipse((282, 227, 742, 704), fill=(109, 157, 87, 255), outline=(226, 190, 105, 255), width=19)
d.polygon([(435, 289), (448, 174), (512, 248), (561, 171), (596, 304)], fill=(156, 181, 88, 255), outline=(226, 190, 105, 255), width=11)
d.polygon([(344, 448), (461, 438), (415, 473), (341, 471)], fill=(243, 200, 80, 255))
d.polygon([(680, 448), (563, 438), (609, 473), (683, 471)], fill=(243, 200, 80, 255))
d.ellipse((477, 515, 547, 565), fill=(50, 88, 60, 255))
d.arc((402, 528, 622, 652), 12, 167, fill=(51, 85, 58, 255), width=17)

# Storage chest in the foreground signals the porter's job at icon scale.
d.rounded_rectangle((238, 644, 786, 847), radius=38, fill=(112, 67, 39, 255), outline=(235, 185, 92, 255), width=20)
d.rectangle((261, 660, 763, 714), fill=(159, 97, 48, 255))
d.line([(512, 648), (512, 843)], fill=(221, 172, 82, 255), width=23)
d.rounded_rectangle((472, 717, 552, 790), radius=12, fill=(238, 196, 99, 255), outline=(79, 51, 36, 255), width=8)

im = im.resize((256, 256), Image.Resampling.LANCZOS)
im.save("packaging/icon.png")
