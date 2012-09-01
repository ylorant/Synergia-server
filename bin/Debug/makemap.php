<?php

$fp = fopen('mymap2.map', 'w+');

//~ fputs($fp, "\x00\x00\x00\x00");

for($i = 0; $i < 25; $i += 4)
{
	for($j = 0; $j < 20; $j += 4)
		fputs($fp, chr($i)."\x00".chr($j)."\x00".str_repeat("\x00", 16));
}

fclose($fp);
