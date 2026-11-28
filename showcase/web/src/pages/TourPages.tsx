import { coreTour } from '../tours/coreTour'
import { moneyTour } from '../tours/moneyTour'
import { Tour } from '../tours/Tour'

export const CoreTourPage = () => <Tour tour={coreTour} />

export const MoneyTourPage = () => <Tour tour={moneyTour} />
