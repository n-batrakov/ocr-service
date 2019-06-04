namespace ITExpert.OcrService.Tesseract
{
    public enum TesseractPageSegmentation
    {
        OnlyOsd = 0,
        AutoWithOsd = 1,
        AutoWithoutOsd = 2,
        FullyAutomaticNoOsd = 3,
        SingleColumn = 4,
        SingleVerticalBlock = 5,
        SingleBlockText = 6,
        SignleLine = 7,
        SingleWord = 8,
        SingleWordCircle = 9,
        SingleChar = 10,
        SparseText = 11,
        SparseTextOsd = 12,
        Raw = 13
    }
}