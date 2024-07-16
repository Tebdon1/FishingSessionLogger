export interface CatchDetails {
    bait: string,
    quantity: number,
    catchWeights: CatchWeights[]
}

interface CatchWeights {
    weight: number
}