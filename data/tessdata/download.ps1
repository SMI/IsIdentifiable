if (test-path "eng.traineddata"){

	return 0
}
ELSE
{
	
	wget "https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata" -outfile "./eng.traineddata"
}
